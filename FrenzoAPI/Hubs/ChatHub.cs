using System.Security.Claims;
using FrenZoAPI.Common.Responses;
using FrenZoAPI.DataContext;
using FrenZoAPI.DTOs.Chat;
using FrenZoAPI.Services;
using FrenZoAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FrenZoAPI.Hubs;

/// <summary>
/// Realtime chat hub (JWT Bearer required via header or <c>?access_token=</c>).
/// Clients join one <c>conversation:{id}</c> group per active membership on connect;
/// <see cref="SendMessage"/> persists the message, then broadcasts it as
/// <c>ReceiveMessage</c> to that group.
/// </summary>
/// <remarks>
/// The exception middleware does not cover hubs, so failures are logged and rethrown as
/// safe HubExceptions (R11: clients get messages, never stacks).
/// </remarks>
[Authorize]
public class ChatHub : Hub
{
    /// <summary>Group-name prefix; full group is <c>conversation:{conversationId}</c>, stable across reconnects.</summary>
    public const string GroupPrefix = "conversation:";

    private readonly AppDbContext _db;
    private readonly IMessageService _messages;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(AppDbContext db, IMessageService messages, ILogger<ChatHub> logger)
    {
        _db = db;
        _messages = messages;
        _logger = logger;
    }

    /// <summary>
    /// Joins the caller's active conversations so broadcasts reach every open tab.
    /// </summary>
    /// <exception cref="HubException">Thrown when the token identity cannot be parsed.</exception>
    public override async Task OnConnectedAsync()
    {
        string? callerIdValue = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Context.User?.FindFirstValue("sub");
        bool isCallerKnown = Guid.TryParse(callerIdValue, out Guid callerId);
        if (!isCallerKnown)
            throw new HubException("Invalid token identity.");

        List<Guid> conversationIds = await _db.TblConversationParticipants.AsNoTracking()
            .Where(p => p.UserId == callerId && !p.IsDelete && p.LeftAt == null)
            .Join(_db.TblConversations.AsNoTracking().Where(c => !c.IsDelete),
                participant => participant.ConversationId,
                conversation => conversation.Id,
                (participant, conversation) => conversation.Id)
            .ToListAsync();

        foreach (Guid conversationId in conversationIds)
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupPrefix + conversationId);

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Persists the message, broadcasts it as <c>ReceiveMessage</c> to the conversation group,
    /// and returns the persisted payload to the caller.
    /// </summary>
    /// <param name="request">Target conversation, trimmed content, and master message-type id.</param>
    /// <returns>The broadcast payload (UTC <c>CreatedAt</c> for clients to localize).</returns>
    /// <exception cref="HubException">
    /// Thrown with a safe message for bad identity, guard failures (not a member, blank,
    /// unknown type), or unexpected errors (logged server-side).
    /// </exception>
    public async Task<MessageBroadcast> SendMessage(SendMessageRequest request)
    {
        string? callerIdValue = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Context.User?.FindFirstValue("sub");
        bool isCallerKnown = Guid.TryParse(callerIdValue, out Guid callerId);
        if (!isCallerKnown)
            throw new HubException("Invalid token identity.");

        try
        {
            ApiResponse<MessageBroadcast> result = await _messages.SendMessageAsync(callerId, request);
            bool isSent = result.Success && result.Data is not null;
            if (!isSent)
                throw new HubException(result.Message);

            MessageBroadcast broadcast = result.Data!;
            await Clients.Group(GroupPrefix + broadcast.ConversationId).SendAsync("ReceiveMessage", broadcast);
            return broadcast;
        }
        catch (HubException)
        {
            throw;
        }
        catch (Exception exception)
        {
            // Unexpected: log the full detail server-side, send a safe message (R11).
            _logger.LogError(exception, "SendMessage failed for conversation {ConversationId}.", request.ConversationId);
            throw new HubException("Could not send the message. Please try again.");
        }
    }
}

using FrenZoAPI.Common.Responses;
using FrenZoAPI.DataContext;
using FrenZoAPI.DTOs.Chat;
using FrenZoAPI.Models;
using FrenZoAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FrenZoAPI.Services;

/// <summary>
/// Persists one chat message for the hub to broadcast (membership-guarded, UTC wall insert).
/// </summary>
public class MessageService : IMessageService
{
    /// <summary>Returned when the caller has no active membership in the conversation.</summary>
    public const string NotMemberMessage = "You are not a member of this conversation.";
    /// <summary>Returned when the message-type id has no active master row.</summary>
    public const string UnknownTypeMessage = "Unknown message type.";
    /// <summary>One message for unknown conversation AND non-member (never leak which one it was).</summary>
    public const string ConversationNotFoundMessage = "Conversation not found.";

    // History page bounds (R9: named, one place to tune).
    private const int DefaultHistoryPageSize = 30;
    private const int MaxHistoryPageSize = 100;

    private readonly AppDbContext _db;

    public MessageService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Validates membership + type, inserts the row, and returns the broadcast payload.
    /// </summary>
    /// <param name="callerId">Sender id from the JWT <c>sub</c> claim.</param>
    /// <param name="request">Target conversation, content, and message-type id.</param>
    /// <returns><c>Ok</c> with the UTC-stamped payload, or <c>Fail</c> (blank/member/type).</returns>
    // No try-catch here: fresh-GUID inserts don't race meaningfully; middleware covers the rest (R11).
    public async Task<ApiResponse<MessageBroadcast>> SendMessageAsync(Guid callerId, SendMessageRequest request)
    {
        string content = request.Content.Trim();
        bool isBlank = content.Length == 0;
        if (isBlank)
            return ApiResponse<MessageBroadcast>.Fail("Message content is required.");

        bool isMember = await _db.TblConversationParticipants.AsNoTracking()
            .AnyAsync(p => p.ConversationId == request.ConversationId && p.UserId == callerId && !p.IsDelete && p.LeftAt == null);
        if (!isMember)
            return ApiResponse<MessageBroadcast>.Fail(NotMemberMessage);

        bool isKnownType = await _db.MstMessageTypes.AsNoTracking()
            .AnyAsync(t => t.Id == request.MessageType && !t.IsDelete);
        if (!isKnownType)
            return ApiResponse<MessageBroadcast>.Fail(UnknownTypeMessage);

        // Unspecified kind keeps the UTC instant storable in 'timestamp without time zone' (R13).
        DateTime nowWall = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        TblMessage message = new TblMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            SenderId = callerId,
            MessageType = request.MessageType,
            Content = content,
            CreatedAt = nowWall,
            IsDelete = false
        };

        _db.TblMessages.Add(message);
        await _db.SaveChangesAsync();

        DateTime createdAtUtc = DateTime.SpecifyKind(message.CreatedAt, DateTimeKind.Utc);
        MessageBroadcast broadcast = new MessageBroadcast
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderId = message.SenderId,
            Content = message.Content,
            MessageType = message.MessageType,
            CreatedAt = createdAtUtc
        };
        return ApiResponse<MessageBroadcast>.Ok(broadcast, "Message sent.");
    }

    /// <summary>
    /// Returns one history page, ascending (oldest→newest) for direct render; fewer than asked means done.
    /// </summary>
    // No try-catch here: read-only path, unexpected failures go to the middleware (R11).
    public async Task<ApiResponse<List<MessageItemResponse>>> GetHistoryAsync(Guid callerId, Guid conversationId, MessageHistoryQuery query)
    {
        bool isMember = await _db.TblConversationParticipants.AsNoTracking()
            .AnyAsync(p => p.ConversationId == conversationId && p.UserId == callerId && !p.IsDelete && p.LeftAt == null);
        if (!isMember)
            return ApiResponse<List<MessageItemResponse>>.Fail(ConversationNotFoundMessage);

        int pageSize = ClampHistoryPageSize(query.PageSize);
        DateTime? cursorWall = NormalizeCursor(query.Cursor);

        IQueryable<TblMessage> messages = _db.TblMessages.AsNoTracking()
            .Where(m => m.ConversationId == conversationId && !m.IsDelete);

        bool hasCursor = cursorWall.HasValue;
        IQueryable<TblMessage> filtered = messages;
        if (hasCursor)
            filtered = messages.Where(m => m.CreatedAt < cursorWall!.Value);

        List<TblMessage> rows = await filtered
            .OrderByDescending(m => m.CreatedAt)
            .Take(pageSize)
            .ToListAsync();

        List<MessageItemResponse> items = new List<MessageItemResponse>(rows.Count);
        for (int index = rows.Count - 1; index >= 0; index--)
        {
            TblMessage row = rows[index];
            DateTime createdAtUtc = DateTime.SpecifyKind(row.CreatedAt, DateTimeKind.Utc);
            MessageItemResponse item = new MessageItemResponse
            {
                Id = row.Id,
                SenderId = row.SenderId,
                Content = row.Content,
                MessageType = row.MessageType,
                CreatedAt = createdAtUtc
            };
            items.Add(item);
        }

        return ApiResponse<List<MessageItemResponse>>.Ok(items, "Messages fetched.");
    }

    /// <summary>
    /// Advances the caller's read marker to the newest message (null when the thread is empty).
    /// </summary>
    // No try-catch here: keyed overwrite converges; middleware covers the rest (R11).
    public async Task<ApiResponse<object>> MarkReadAsync(Guid callerId, Guid conversationId)
    {
        TblConversationParticipant? member = await _db.TblConversationParticipants
            .FirstOrDefaultAsync(p => p.ConversationId == conversationId && p.UserId == callerId && !p.IsDelete && p.LeftAt == null);
        if (member is null)
            return ApiResponse<object>.Fail(ConversationNotFoundMessage);

        Guid? newestId = await _db.TblMessages.AsNoTracking()
            .Where(m => m.ConversationId == conversationId && !m.IsDelete)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => (Guid?)m.Id)
            .FirstOrDefaultAsync();

        member.LastReadMessageId = newestId;
        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(null, "Marked as read.");
    }

    // Keeps history pages inside the contract (R6: comparisons stay out of call arguments).
    private static int ClampHistoryPageSize(int requested)
    {
        bool isTooSmall = requested < 1;
        if (isTooSmall)
            return DefaultHistoryPageSize;
        bool isTooLarge = requested > MaxHistoryPageSize;
        if (isTooLarge)
            return MaxHistoryPageSize;
        return requested;
    }

    // Treats the cursor as a UTC instant wall for without-tz comparison (null starts newest).
    private static DateTime? NormalizeCursor(DateTime? cursor)
    {
        bool hasCursor = cursor.HasValue;
        if (!hasCursor)
            return null;
        DateTime universal = cursor!.Value.ToUniversalTime();
        DateTime cursorWall = DateTime.SpecifyKind(universal, DateTimeKind.Unspecified);
        return cursorWall;
    }
}

using System.Security.Claims;
using FrenZoAPI.Common.Responses;
using FrenZoAPI.DTOs.Chat;
using FrenZoAPI.Services;
using FrenZoAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrenZoAPI.Controllers;

// Chat reads and writes for the signed-in caller (JWT Bearer required).
[Authorize]
[ApiController]
[Route("api/conversations")]
public class ConversationsController : ControllerBase
{
    private readonly IConversationService _conversations;
    private readonly IMessageService _messages;

    public ConversationsController(IConversationService conversations, IMessageService messages)
    {
        _conversations = conversations;
        _messages = messages;
    }

    // GET api/conversations?cursor=&pageSize= -> 200 page, 401 unknown/expired token.
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ChatlistItemResponse>>>> GetChatlist([FromQuery] ChatlistQuery query)
    {
        string? callerIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        bool isCallerKnown = Guid.TryParse(callerIdValue, out Guid callerId);
        if (!isCallerKnown)
            return Unauthorized(ApiResponse<List<ChatlistItemResponse>>.Fail("Invalid token identity."));

        ApiResponse<List<ChatlistItemResponse>> result = await _conversations.GetChatlistAsync(callerId, query);
        return Ok(result);
    }

    // POST api/conversations/direct { otherUserId } -> 200 opened/started, 400 validation/self, 404 unknown user.
    [HttpPost("direct")]
    public async Task<ActionResult<ApiResponse<ChatlistItemResponse>>> StartDirect([FromBody] StartDirectRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<ChatlistItemResponse>.Fail("Validation failed.", ModelState));

        string? callerIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        bool isCallerKnown = Guid.TryParse(callerIdValue, out Guid callerId);
        if (!isCallerKnown)
            return Unauthorized(ApiResponse<ChatlistItemResponse>.Fail("Invalid token identity."));

        ApiResponse<ChatlistItemResponse> result = await _conversations.StartDirectAsync(callerId, request.OtherUserId);
        bool isSucceeded = result.Success;
        if (isSucceeded)
            return Ok(result);

        bool isSelfChat = result.Message == ConversationService.SelfChatMessage;
        if (isSelfChat)
            return BadRequest(result);

        bool isUnknownUser = result.Message == ConversationService.UnknownUserMessage;
        if (isUnknownUser)
            return NotFound(result);

        return Conflict(result);
    }

    // GET api/conversations/{id}/messages?cursor=&pageSize= -> 200 page, 401, 404 unknown/non-member.
    [HttpGet("{id:guid}/messages")]
    public async Task<ActionResult<ApiResponse<List<MessageItemResponse>>>> GetHistory(Guid id, [FromQuery] MessageHistoryQuery query)
    {
        string? callerIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        bool isCallerKnown = Guid.TryParse(callerIdValue, out Guid callerId);
        if (!isCallerKnown)
            return Unauthorized(ApiResponse<List<MessageItemResponse>>.Fail("Invalid token identity."));

        ApiResponse<List<MessageItemResponse>> result = await _messages.GetHistoryAsync(callerId, id, query);
        bool isSucceeded = result.Success;
        if (!isSucceeded)
            return NotFound(result);

        return Ok(result);
    }

    // POST api/conversations/{id}/read -> 200 marked, 401, 404 unknown/non-member.
    [HttpPost("{id:guid}/read")]
    public async Task<ActionResult<ApiResponse<object>>> MarkRead(Guid id)
    {
        string? callerIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        bool isCallerKnown = Guid.TryParse(callerIdValue, out Guid callerId);
        if (!isCallerKnown)
            return Unauthorized(ApiResponse<object>.Fail("Invalid token identity."));

        ApiResponse<object> result = await _messages.MarkReadAsync(callerId, id);
        bool isSucceeded = result.Success;
        if (!isSucceeded)
            return NotFound(result);

        return Ok(result);
    }
}

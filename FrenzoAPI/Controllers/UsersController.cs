using System.Security.Claims;
using FrenZoAPI.Common.Responses;
using FrenZoAPI.DTOs.Chat;
using FrenZoAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrenZoAPI.Controllers;

// User picker for starting chats (JWT Bearer required; self never listed).
[Authorize]
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IConversationService _conversations;

    public UsersController(IConversationService conversations)
    {
        _conversations = conversations;
    }

    // GET api/users?search=&pageSize= -> 200 page, 401 unknown/expired token.
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<UserListItemResponse>>>> GetUsers([FromQuery] UserListQuery query)
    {
        string? callerIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        bool isCallerKnown = Guid.TryParse(callerIdValue, out Guid callerId);
        if (!isCallerKnown)
            return Unauthorized(ApiResponse<List<UserListItemResponse>>.Fail("Invalid token identity."));

        ApiResponse<List<UserListItemResponse>> result = await _conversations.GetUsersAsync(callerId, query);
        return Ok(result);
    }
}

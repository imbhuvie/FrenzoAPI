using FrenZoAPI.Common.Responses;
using FrenZoAPI.DTOs.Chat;

namespace FrenZoAPI.Services.Interfaces;

// Chatlist and conversation reads for the caller's own memberships.
public interface IConversationService
{
    // Returns the caller's conversations newest-first with last-message previews.
    Task<ApiResponse<List<ChatlistItemResponse>>> GetChatlistAsync(Guid callerId, ChatlistQuery query);
}

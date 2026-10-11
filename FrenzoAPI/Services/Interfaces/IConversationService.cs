using FrenZoAPI.Common.Responses;
using FrenZoAPI.DTOs.Chat;

namespace FrenZoAPI.Services.Interfaces;

// Chatlist and conversation reads for the caller's own memberships.
public interface IConversationService
{
    // Returns the caller's conversations newest-first with last-message previews.
    Task<ApiResponse<List<ChatlistItemResponse>>> GetChatlistAsync(Guid callerId, ChatlistQuery query);

    // Lists active users (self excluded) with existing-direct-chat flags for the picker.
    Task<ApiResponse<List<UserListItemResponse>>> GetUsersAsync(Guid callerId, UserListQuery query);

    // Opens the existing Private chat or creates it (idempotent).
    Task<ApiResponse<ChatlistItemResponse>> StartDirectAsync(Guid callerId, Guid otherUserId);
}

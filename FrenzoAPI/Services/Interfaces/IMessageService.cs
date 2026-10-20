using FrenZoAPI.Common.Responses;
using FrenZoAPI.DTOs.Chat;

namespace FrenZoAPI.Services.Interfaces;

/// <summary>
/// Message persistence for the realtime path (the hub broadcasts what this returns).
/// </summary>
public interface IMessageService
{
    /// <summary>
    /// Validates membership + type, inserts the row, and returns the broadcast payload.
    /// </summary>
    Task<ApiResponse<MessageBroadcast>> SendMessageAsync(Guid callerId, SendMessageRequest request);

    /// <summary>
    /// Returns one history page, ascending (oldest→newest) for direct render; fewer than asked means done.
    /// </summary>
    Task<ApiResponse<List<MessageItemResponse>>> GetHistoryAsync(Guid callerId, Guid conversationId, MessageHistoryQuery query);

    /// <summary>
    /// Advances the caller's read marker to the newest message (null when the thread is empty).
    /// </summary>
    Task<ApiResponse<object>> MarkReadAsync(Guid callerId, Guid conversationId);
}

namespace FrenZoAPI.DTOs.Chat;

// One row of the start-chat user picker (profile fields fall back to the username).
public class UserListItemResponse
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public bool HasDirectChat { get; set; }
    public Guid? DirectConversationId { get; set; }
}

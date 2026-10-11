namespace FrenZoAPI.DTOs.Chat;

// One chatlist UI row: who, what last happened, and what needs attention.
public class ChatlistItemResponse
{
    public Guid ConversationId { get; set; }
    public short Type { get; set; }
    public bool IsGroup { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public bool IsMuted { get; set; }
    public bool IsAdmin { get; set; }
    public int UnreadCount { get; set; }
    public LastMessageResponse? LastMessage { get; set; }
    public DateTime UpdatedAt { get; set; }
}

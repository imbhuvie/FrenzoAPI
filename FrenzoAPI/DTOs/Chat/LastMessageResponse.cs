namespace FrenZoAPI.DTOs.Chat;

// Last message preview inside a chatlist row; null when the conversation has no messages yet.
public class LastMessageResponse
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid SenderId { get; set; }
    public short MessageType { get; set; }
    public DateTime CreatedAt { get; set; }
}

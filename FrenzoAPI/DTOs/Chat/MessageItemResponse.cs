namespace FrenZoAPI.DTOs.Chat;

// One history row (same shape as the live broadcast, UTC instant for clients to localize).
public class MessageItemResponse
{
    /// <summary>Persisted message id (also the read-marker value).</summary>
    public Guid Id { get; set; }
    /// <summary>Sender id (compare with your own id for bubble side).</summary>
    public Guid SenderId { get; set; }
    /// <summary>Message text.</summary>
    public string Content { get; set; } = string.Empty;
    /// <summary>Master message-type id (seed: 1=Text … 5=File).</summary>
    public short MessageType { get; set; }
    /// <summary>UTC instant for clients to localize (R13).</summary>
    public DateTime CreatedAt { get; set; }
}

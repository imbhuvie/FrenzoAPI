namespace FrenZoAPI.DTOs.Chat;

/// <summary>
/// <c>ReceiveMessage</c> payload: the persisted row shaped for broadcast (never internal fields).
/// </summary>
public class MessageBroadcast
{
    /// <summary>Persisted message id (echoed back to the sender too).</summary>
    public Guid Id { get; set; }
    /// <summary>Owning conversation (also the broadcast group key).</summary>
    public Guid ConversationId { get; set; }
    /// <summary>Sender id from the JWT <c>sub</c> claim.</summary>
    public Guid SenderId { get; set; }
    /// <summary>Trimmed message text.</summary>
    public string Content { get; set; } = string.Empty;
    /// <summary>Master message-type id (seed: 1=Text … 5=File).</summary>
    public short MessageType { get; set; }
    /// <summary>UTC instant for clients to localize (R13).</summary>
    public DateTime CreatedAt { get; set; }
}

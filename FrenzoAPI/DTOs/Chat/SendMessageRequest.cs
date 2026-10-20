using System.ComponentModel.DataAnnotations;

namespace FrenZoAPI.DTOs.Chat;

/// <summary>
/// Hub <c>SendMessage</c> input: one text-first message into a conversation the caller joined.
/// </summary>
public class SendMessageRequest
{
    /// <summary>Target conversation the caller must actively belong to.</summary>
    [Required]
    public Guid ConversationId { get; set; }

    /// <summary>Trimmed before save; 1–4000 chars.</summary>
    [Required, MinLength(1), MaxLength(4000)]
    public string Content { get; set; } = string.Empty;

    /// <summary>Master id from mst_message_type seed (1=Text, 2=Image, 3=Video, 4=Audio, 5=File).</summary>
    [Range(1, 5)]
    public short MessageType { get; set; } = 1;
}

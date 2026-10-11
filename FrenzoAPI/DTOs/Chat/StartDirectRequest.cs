using System.ComponentModel.DataAnnotations;

namespace FrenZoAPI.DTOs.Chat;

// Input for POST api/conversations/direct: open-or-create a Private chat with one user.
public class StartDirectRequest
{
    [Required]
    public Guid OtherUserId { get; set; }
}

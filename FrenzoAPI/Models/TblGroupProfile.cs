using System;
using System.Collections.Generic;

namespace FrenZoAPI.Models;

public partial class TblGroupProfile
{
    public Guid ConversationId { get; set; }

    public string GroupName { get; set; } = null!;

    public string? Description { get; set; }

    public string? GroupPhotoUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDelete { get; set; }

    public virtual TblConversation Conversation { get; set; } = null!;
}

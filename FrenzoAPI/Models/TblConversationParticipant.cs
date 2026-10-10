using System;
using System.Collections.Generic;

namespace FrenZoAPI.Models;

public partial class TblConversationParticipant
{
    public Guid ConversationId { get; set; }

    public Guid UserId { get; set; }

    public DateTime JoinedAt { get; set; }

    public DateTime? LeftAt { get; set; }

    public bool IsDelete { get; set; }

    public Guid? LastReadMessageId { get; set; }

    public DateTime? MutedUntil { get; set; }

    public bool IsAdmin { get; set; }

    public virtual TblConversation Conversation { get; set; } = null!;

    public virtual TblMessage? LastReadMessage { get; set; }

    public virtual TblUser User { get; set; } = null!;
}

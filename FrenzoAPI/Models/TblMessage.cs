using System;
using System.Collections.Generic;

namespace FrenZoAPI.Models;

public partial class TblMessage
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    public Guid SenderId { get; set; }

    public short MessageType { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool IsDelete { get; set; }

    public virtual TblConversation Conversation { get; set; } = null!;

    public virtual MstMessageType MessageTypeNavigation { get; set; } = null!;

    public virtual TblUser Sender { get; set; } = null!;
}

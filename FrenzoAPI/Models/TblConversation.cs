using System;
using System.Collections.Generic;

namespace FrenZoAPI.Models;

public partial class TblConversation
{
    public Guid Id { get; set; }

    public short Type { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDelete { get; set; }

    public virtual TblUser CreatedByNavigation { get; set; } = null!;

    public virtual TblGroupProfile? TblGroupProfile { get; set; }

    public virtual ICollection<TblMessage> TblMessages { get; set; } = new List<TblMessage>();

    public virtual MstConversationType TypeNavigation { get; set; } = null!;
}

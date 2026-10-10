using System;
using System.Collections.Generic;

namespace FrenZoAPI.Models;

public partial class MstConversationType
{
    public short Id { get; set; }

    public string ConversationType { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDelete { get; set; }

    public virtual ICollection<TblConversation> TblConversations { get; set; } = new List<TblConversation>();
}

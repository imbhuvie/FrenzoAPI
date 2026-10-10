using System;
using System.Collections.Generic;

namespace FrenZoAPI.Models;

public partial class MstMessageType
{
    public short Id { get; set; }

    public string MessageType { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDelete { get; set; }

    public virtual ICollection<TblMessage> TblMessages { get; set; } = new List<TblMessage>();
}

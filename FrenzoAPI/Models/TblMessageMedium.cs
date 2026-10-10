using System;
using System.Collections.Generic;

namespace FrenZoAPI.Models;

public partial class TblMessageMedium
{
    public Guid MessageId { get; set; }

    public Guid MediaId { get; set; }

    public bool IsDelete { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual TblMedium Media { get; set; } = null!;

    public virtual TblMessage Message { get; set; } = null!;
}

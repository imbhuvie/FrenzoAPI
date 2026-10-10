using System;
using System.Collections.Generic;

namespace FrenZoAPI.Models;

public partial class MstMessageStatus
{
    public short Id { get; set; }

    public string MessageStatus { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDelete { get; set; }
}

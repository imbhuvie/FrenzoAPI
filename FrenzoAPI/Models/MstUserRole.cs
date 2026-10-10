using System;
using System.Collections.Generic;

namespace FrenZoAPI.Models;

public partial class MstUserRole
{
    public short Id { get; set; }

    public string Role { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDelete { get; set; }
}

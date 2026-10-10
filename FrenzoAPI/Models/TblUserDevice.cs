using System;
using System.Collections.Generic;

namespace FrenZoAPI.Models;

public partial class TblUserDevice
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string? DeviceType { get; set; }

    public string? DeviceToken { get; set; }

    public DateTime LastActiveAt { get; set; }

    public bool IsDelete { get; set; }

    public virtual TblUser User { get; set; } = null!;
}

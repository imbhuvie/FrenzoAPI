using System;
using System.Collections.Generic;

namespace FrenZoAPI.Models;

public partial class UserProfile
{
    public Guid UserId { get; set; }

    public string DisplayName { get; set; } = null!;

    public string? Bio { get; set; }

    public string? ProfilePhotoUrl { get; set; }

    public DateOnly? Dob { get; set; }

    public short? Gender { get; set; }

    public string? Country { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDelete { get; set; }

    public bool IsOnline { get; set; }

    public DateTime LastSeen { get; set; }

    public string? Address { get; set; }

    public virtual TblUser User { get; set; } = null!;
}

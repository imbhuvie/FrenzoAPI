using System;
using System.Collections.Generic;

namespace FrenZoAPI.Models;

public partial class TblUser
{
    public Guid Id { get; set; }

    public string Username { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDelete { get; set; }

    public virtual ICollection<TblConversation> TblConversations { get; set; } = new List<TblConversation>();

    public virtual ICollection<TblMessage> TblMessages { get; set; } = new List<TblMessage>();

    public virtual ICollection<TblUserDevice> TblUserDevices { get; set; } = new List<TblUserDevice>();

    public virtual UserProfile? UserProfile { get; set; }
}

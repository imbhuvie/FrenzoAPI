using System.ComponentModel.DataAnnotations;

namespace FrenZoAPI.DTOs.Auth;

// Input for POST api/auth/signup; all fields map 1-1 to required tbl_users columns.
public class SignupRequest
{
    [Required, MinLength(3), MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required, Phone, MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, MinLength(6), MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}

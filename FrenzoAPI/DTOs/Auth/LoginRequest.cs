using System.ComponentModel.DataAnnotations;

namespace FrenZoAPI.DTOs.Auth;

// Input for POST api/auth/login; Identifier accepts username, email, or phone.
public class LoginRequest
{
    [Required, MaxLength(255)]
    public string Identifier { get; set; } = string.Empty;

    [Required, MinLength(6), MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}

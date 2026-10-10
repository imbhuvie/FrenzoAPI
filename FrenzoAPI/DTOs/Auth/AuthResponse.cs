namespace FrenZoAPI.DTOs.Auth;

// Login success payload: safe user plus the Bearer token (access only, no refresh yet).
public class AuthResponse
{
    public UserResponse User { get; set; } = new UserResponse();
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

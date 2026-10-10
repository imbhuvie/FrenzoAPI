using FrenZoAPI.Models;

namespace FrenZoAPI.Services.Interfaces;

// Issues signed JWT access tokens; validation itself is done by the JwtBearer middleware.
public interface ITokenService
{
    // Builds a token for an already-verified user plus its UTC expiry.
    (string Token, DateTime ExpiresAt) GenerateAccessToken(TblUser user);
}

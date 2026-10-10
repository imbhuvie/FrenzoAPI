using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FrenZoAPI.Models;
using FrenZoAPI.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace FrenZoAPI.Services;

// Signs short-lived access tokens with the Jwt:Key secret (HMAC-SHA256).
public class TokenService : ITokenService
{
    // Fallback lifetime when Jwt:AccessTokenMinutes is missing or invalid.
    private const int DefaultAccessTokenMinutes = 60;

    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Builds a token for an already-verified user plus its UTC expiry.
    public (string Token, DateTime ExpiresAt) GenerateAccessToken(TblUser user)
    {
        string? configuredKey = _configuration["Jwt:Key"];
        bool isKeyMissing = string.IsNullOrWhiteSpace(configuredKey);
        if (isKeyMissing)
            throw new InvalidOperationException("Jwt:Key is missing. Set it in appsettings or User Secrets.");

        string issuer = _configuration["Jwt:Issuer"] ?? "FrenZoAPI";
        string audience = _configuration["Jwt:Audience"] ?? "FrenZoAPI";
        int lifetimeMinutes = ReadLifetimeMinutes();

        DateTime expiresAt = DateTime.UtcNow.AddMinutes(lifetimeMinutes);

        // Claims identify the caller on later requests; never include secrets or hashes.
        Claim[] claims = new Claim[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuredKey!));
        SigningCredentials credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken jwt = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
        string token = handler.WriteToken(jwt);
        return (token, expiresAt);
    }

    // Reads Jwt:AccessTokenMinutes with a safe fallback (R6: calls stay out of conditions).
    private int ReadLifetimeMinutes()
    {
        string? configuredValue = _configuration["Jwt:AccessTokenMinutes"];
        bool isParsed = int.TryParse(configuredValue, out int parsedMinutes);
        bool isPositive = parsedMinutes > 0;
        bool useConfiguredValue = isParsed && isPositive;
        if (useConfiguredValue)
            return parsedMinutes;
        return DefaultAccessTokenMinutes;
    }
}

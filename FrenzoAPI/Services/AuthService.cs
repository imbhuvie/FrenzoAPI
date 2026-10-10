using FrenZoAPI.Common.Responses;
using FrenZoAPI.DataContext;
using FrenZoAPI.DTOs.Auth;
using FrenZoAPI.Models;
using FrenZoAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FrenZoAPI.Services;

// Handles signup/login rules on top of tbl_users (BCrypt + EF, no JWT yet).
public class AuthService : IAuthService
{
    // BCrypt cost; ~100ms per hash on current hardware. Raise to harden, lower to speed up.
    private const int PasswordHashWorkFactor = 11;

    private readonly AppDbContext _db;

    public AuthService(AppDbContext db)
    {
        _db = db;
    }

    // Registers a new user row with a BCrypt hash (no UserProfile yet).
    public async Task<ApiResponse<UserResponse>> SignupAsync(SignupRequest request)
    {
        // Normalize inputs so duplicates compare consistently.
        string username = request.Username.Trim();
        string email = request.Email.Trim().ToLowerInvariant();
        string phone = request.PhoneNumber.Trim();

        // Check each unique field once up front (R6: no calls inside conditions).
        bool isUsernameTaken = await _db.TblUsers.AnyAsync(u => !u.IsDelete && u.Username == username);
        bool isEmailTaken = await _db.TblUsers.AnyAsync(u => !u.IsDelete && u.Email == email);
        bool isPhoneTaken = await _db.TblUsers.AnyAsync(u => !u.IsDelete && u.PhoneNumber == phone);

        if (isUsernameTaken)
            return ApiResponse<UserResponse>.Fail("Username already exists.");

        if (isEmailTaken)
            return ApiResponse<UserResponse>.Fail("Email already exists.");

        if (isPhoneTaken)
            return ApiResponse<UserResponse>.Fail("Phone number already exists.");

        // Unspecified kind keeps UTC instant storable in 'timestamp without time zone'.
        DateTime now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        TblUser user = new TblUser
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            PhoneNumber = phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, PasswordHashWorkFactor),
            CreatedAt = now,
            UpdatedAt = now,
            IsDelete = false
        };

        _db.TblUsers.Add(user);
        await _db.SaveChangesAsync();

        return ApiResponse<UserResponse>.Ok(Map(user), "Signup successful.");
    }

    // Verifies username/email/phone + password without revealing which part failed.
    public async Task<ApiResponse<UserResponse>> LoginAsync(LoginRequest request)
    {
        string identifier = request.Identifier.Trim();
        string identifierLower = identifier.ToLowerInvariant();

        // Single lookup across all three unique login fields.
        TblUser? user = await _db.TblUsers.FirstOrDefaultAsync(u =>
            !u.IsDelete &&
            (u.Email == identifierLower || u.Username == identifier || u.PhoneNumber == identifier));

        // Same message for not-found and bad-password to avoid user enumeration.
        bool isPasswordValid = user is not null && BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (user is null || !isPasswordValid)
            return ApiResponse<UserResponse>.Fail("Invalid credentials.");

        return ApiResponse<UserResponse>.Ok(Map(user), "Login successful.");
    }

    // Maps entity to safe output (never exposes PasswordHash).
    private static UserResponse Map(TblUser u) => new()
    {
        Id = u.Id,
        Username = u.Username,
        Email = u.Email,
        PhoneNumber = u.PhoneNumber,
        CreatedAt = u.CreatedAt
    };
}

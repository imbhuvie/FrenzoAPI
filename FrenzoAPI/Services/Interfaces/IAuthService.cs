using FrenZoAPI.Common.Responses;
using FrenZoAPI.DTOs.Auth;

namespace FrenZoAPI.Services.Interfaces;

// Auth business-rule contract; controllers stay thin and testable.
public interface IAuthService
{
    // Creates the TblUser row and returns the safe user payload.
    Task<ApiResponse<UserResponse>> SignupAsync(SignupRequest request);
    // Checks credentials and returns the safe user payload.
    Task<ApiResponse<UserResponse>> LoginAsync(LoginRequest request);
}

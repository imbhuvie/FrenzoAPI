using FrenZoAPI.Common.Responses;
using FrenZoAPI.DTOs.Auth;
using FrenZoAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FrenZoAPI.Controllers;

// Open auth endpoints (no JWT yet): signup and login on tbl_users.
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    // POST api/auth/signup -> 200 ok, 400 validation, 409 duplicate.
    [HttpPost("signup")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Signup([FromBody] SignupRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<UserResponse>.Fail("Validation failed.", ModelState));

        ApiResponse<UserResponse> result = await _auth.SignupAsync(request);

        if (!result.Success)
            return Conflict(result);

        return Ok(result);
    }

    // POST api/auth/login -> 200 ok, 400 validation, 401 bad credentials.
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<UserResponse>.Fail("Validation failed.", ModelState));

        ApiResponse<UserResponse> result = await _auth.LoginAsync(request);

        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }
}

using System.Security.Claims;
using GOKCafe.Application.DTOs.Auth;
using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GOKCafe.API.Controllers;

/// <summary>
/// Manages user authentication and account operations
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[ApiExplorerSettings(GroupName = "Authentication API")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="dto">Registration details</param>
    /// <returns>Authentication response with JWT token</returns>
    [HttpPost("register")]
    [ProducesResponseType<ApiResponse<AuthResponseDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiResponse<AuthResponseDto>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return result.Success
            ? CreatedAtAction(nameof(GetProfile), new { }, result)
            : BadRequest(result);
    }

    /// <summary>
    /// Login to an existing account
    /// </summary>
    /// <param name="dto">Login credentials</param>
    /// <returns>Authentication response with JWT token</returns>
    [HttpPost("login")]
    [ProducesResponseType<ApiResponse<AuthResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<AuthResponseDto>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get current user profile (requires authentication)
    /// </summary>
    /// <returns>User profile information</returns>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType<ApiResponse<UserDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<UserDto>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<UserDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetCurrentUserId();
        var result = await _authService.GetProfileAsync(userId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Update user profile (requires authentication)
    /// </summary>
    /// <param name="dto">Updated profile information</param>
    /// <returns>Updated user profile</returns>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType<ApiResponse<UserDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<UserDto>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<UserDto>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<UserDto>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _authService.UpdateProfileAsync(userId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Change user password (requires authentication)
    /// </summary>
    /// <param name="dto">Password change details</param>
    /// <returns>Success status</returns>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _authService.ChangePasswordAsync(userId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Deactivate user account (requires authentication)
    /// </summary>
    /// <returns>Success status</returns>
    [HttpDelete("deactivate")]
    [Authorize]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeactivateAccount()
    {
        var userId = GetCurrentUserId();
        var result = await _authService.DeactivateAccountAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}

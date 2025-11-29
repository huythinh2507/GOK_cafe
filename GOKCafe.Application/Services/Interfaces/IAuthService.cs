using GOKCafe.Application.DTOs.Auth;
using GOKCafe.Application.DTOs.Common;

namespace GOKCafe.Application.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto);
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto);
    Task<ApiResponse<bool>> LogoutAsync(string token, Guid userId);
    Task<bool> IsTokenRevokedAsync(string token);
    Task<ApiResponse<UserDto>> GetProfileAsync(Guid userId);
    Task<ApiResponse<UserDto>> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
    Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
    Task<ApiResponse<bool>> DeactivateAccountAsync(Guid userId);
}

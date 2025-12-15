using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GOKCafe.Application.DTOs.Auth;
using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.Services.Interfaces;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;
using GOKCafe.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GOKCafe.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _unitOfWork.Users.GetQueryable()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (existingUser != null)
            {
                return ApiResponse<AuthResponseDto>.FailureResult(
                    "Email already registered",
                    new List<string> { "A user with this email already exists" });
            }

            // Create new user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = dto.Email.ToLower(),
                PasswordHash = _passwordHasher.HashPassword(dto.Password),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                Role = UserRole.Customer, // Default role
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Generate token
            var token = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddHours(GetTokenExpirationHours());

            var response = new AuthResponseDto
            {
                Token = token,
                User = MapToUserDto(user),
                ExpiresAt = expiresAt
            };

            return ApiResponse<AuthResponseDto>.SuccessResult(
                response,
                "Registration successful");
        }
        catch (Exception ex)
        {
            return ApiResponse<AuthResponseDto>.FailureResult(
                "An error occurred during registration",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        try
        {
            // Find user by email
            var user = await _unitOfWork.Users.GetQueryable()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (user == null)
            {
                return ApiResponse<AuthResponseDto>.FailureResult(
                    "Invalid credentials",
                    new List<string> { "Email or password is incorrect" });
            }

            // Check if account is active
            if (!user.IsActive)
            {
                return ApiResponse<AuthResponseDto>.FailureResult(
                    "Account deactivated",
                    new List<string> { "This account has been deactivated. Please contact support." });
            }

            // Verify password
            if (!_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
            {
                return ApiResponse<AuthResponseDto>.FailureResult(
                    "Invalid credentials",
                    new List<string> { "Email or password is incorrect" });
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            // Generate token
            var token = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddHours(GetTokenExpirationHours());

            var response = new AuthResponseDto
            {
                Token = token,
                User = MapToUserDto(user),
                ExpiresAt = expiresAt
            };

            return ApiResponse<AuthResponseDto>.SuccessResult(
                response,
                "Login successful");
        }
        catch (Exception ex)
        {
            return ApiResponse<AuthResponseDto>.FailureResult(
                "An error occurred during login",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<bool>> LogoutAsync(string token, Guid userId)
    {
        try
        {
            // Extract expiration from token
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var expiresAt = jwtToken.ValidTo;

            // Add token to revoked tokens list
            var revokedToken = new RevokedToken
            {
                Id = Guid.NewGuid(),
                Token = token,
                RevokedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                UserId = userId,
                Reason = "User logout"
            };

            await _unitOfWork.RevokedTokens.AddAsync(revokedToken);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true, "Logout successful");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.FailureResult(
                "An error occurred during logout",
                new List<string> { ex.Message });
        }
    }

    public async Task<bool> IsTokenRevokedAsync(string token)
    {
        try
        {
            var revokedToken = await _unitOfWork.RevokedTokens.GetQueryable()
                .FirstOrDefaultAsync(rt => rt.Token == token && rt.ExpiresAt > DateTime.UtcNow);

            return revokedToken != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ApiResponse<UserDto>> GetProfileAsync(Guid userId)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null)
            {
                return ApiResponse<UserDto>.FailureResult("User not found");
            }

            return ApiResponse<UserDto>.SuccessResult(MapToUserDto(user));
        }
        catch (Exception ex)
        {
            return ApiResponse<UserDto>.FailureResult(
                "An error occurred while retrieving profile",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<UserDto>> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null)
            {
                return ApiResponse<UserDto>.FailureResult("User not found");
            }

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.PhoneNumber = dto.PhoneNumber;
            user.Address = dto.Address;
            user.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<UserDto>.SuccessResult(
                MapToUserDto(user),
                "Profile updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<UserDto>.FailureResult(
                "An error occurred while updating profile",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null)
            {
                return ApiResponse<bool>.FailureResult("User not found");
            }

            // Verify current password
            if (!_passwordHasher.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
            {
                return ApiResponse<bool>.FailureResult(
                    "Invalid current password",
                    new List<string> { "The current password is incorrect" });
            }

            // Update password
            user.PasswordHash = _passwordHasher.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true, "Password changed successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.FailureResult(
                "An error occurred while changing password",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<bool>> DeactivateAccountAsync(Guid userId)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null)
            {
                return ApiResponse<bool>.FailureResult("User not found");
            }

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true, "Account deactivated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.FailureResult(
                "An error occurred while deactivating account",
                new List<string> { ex.Message });
        }
    }

    private string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT secret key not configured")));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, ((int)user.Role).ToString()),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(GetTokenExpirationHours()),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private double GetTokenExpirationHours()
    {
        return double.TryParse(_configuration["Jwt:ExpirationHours"], out var hours) ? hours : 24;
    }

    private UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        };
    }
}

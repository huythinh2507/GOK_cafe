using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GOKCafe.Application.DTOs.Auth;
using GOKCafe.Application.DTOs.Common;
using GOKCafe.Application.Services.Interfaces;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;
using GOKCafe.Infrastructure.Services;
using GOKCafe.Infrastructure.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Google.Apis.Auth;

namespace GOKCafe.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _emailService = emailService;
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

    public async Task<ApiResponse<AuthResponseDto>> GoogleLoginAsync(GoogleLoginDto dto)
    {
        try
        {
            // Verify Google ID token
            var payload = await VerifyGoogleTokenAsync(dto.IdToken);

            if (payload == null)
            {
                return ApiResponse<AuthResponseDto>.FailureResult(
                    "Invalid Google token",
                    new List<string> { "The provided Google ID token is invalid or expired" });
            }

            // Check if user exists
            var user = await _unitOfWork.Users.GetQueryable()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == payload.Email.ToLower());

            if (user == null)
            {
                // Create new user from Google profile
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = payload.Email,
                    FirstName = payload.GivenName ?? "User",
                    LastName = payload.FamilyName ?? "",
                    PasswordHash = _passwordHasher.HashPassword(Guid.NewGuid().ToString()), // Random password for Google users
                    Role = UserRole.Customer,
                    IsActive = true,
                    LastLoginAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                // Send welcome email
                var userName = $"{user.FirstName} {user.LastName}";
                _ = _emailService.SendWelcomeEmailAsync(user.Email, userName); // Fire and forget
            }
            else
            {
                // Check if account is active
                if (!user.IsActive)
                {
                    return ApiResponse<AuthResponseDto>.FailureResult(
                        "Account deactivated",
                        new List<string> { "This account has been deactivated. Please contact support." });
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();
            }

            // Generate JWT token
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
                "Google login successful");
        }
        catch (Exception ex)
        {
            return ApiResponse<AuthResponseDto>.FailureResult(
                "An error occurred during Google login",
                new List<string> { ex.Message });
        }
    }

    private async Task<GoogleJsonWebSignature.Payload?> VerifyGoogleTokenAsync(string idToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _configuration["GoogleAuth:ClientId"] ?? "" }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            return payload;
        }
        catch
        {
            return null;
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

    public async Task<ApiResponse<ForgotPasswordResponseDto>> ForgotPasswordAsync(ForgotPasswordDto dto, string resetBaseUrl)
    {
        try
        {
            var user = await _unitOfWork.Users.GetQueryable()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower() && u.IsActive);

            // Always return success to prevent email enumeration attacks
            if (user == null)
            {
                return ApiResponse<ForgotPasswordResponseDto>.SuccessResult(
                    new ForgotPasswordResponseDto
                    {
                        Message = "If an account with that email exists, a password reset link has been sent."
                    },
                    "Password reset email sent");
            }

            // Generate secure reset token
            var resetToken = GeneratePasswordResetToken();
            var tokenExpiry = DateTime.UtcNow.AddHours(1); // Token valid for 1 hour

            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiry = tokenExpiry;
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            // Send password reset email
            var resetUrl = $"{resetBaseUrl}?token={resetToken}&email={Uri.EscapeDataString(user.Email)}";
            var userName = $"{user.FirstName} {user.LastName}";

            var emailSent = await _emailService.SendPasswordResetEmailAsync(user.Email, userName, resetToken, resetUrl);

            var isDevelopment = _configuration["ASPNETCORE_ENVIRONMENT"] == "Development";

            return ApiResponse<ForgotPasswordResponseDto>.SuccessResult(
                new ForgotPasswordResponseDto
                {
                    Message = "If an account with that email exists, a password reset link has been sent.",
                    ResetToken = isDevelopment ? resetToken : null // Only for dev/testing
                },
                emailSent ? "Password reset email sent successfully" : "Password reset token generated (email sending failed)");
        }
        catch (Exception ex)
        {
            return ApiResponse<ForgotPasswordResponseDto>.FailureResult(
                "An error occurred while processing your request",
                new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordDto dto)
    {
        try
        {
            var user = await _unitOfWork.Users.GetQueryable()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower() && u.IsActive);

            if (user == null)
            {
                return ApiResponse<bool>.FailureResult(
                    "Invalid reset token or email",
                    new List<string> { "The reset token is invalid or has expired" });
            }

            // Verify reset token
            if (string.IsNullOrEmpty(user.PasswordResetToken) ||
                user.PasswordResetToken != dto.Token)
            {
                return ApiResponse<bool>.FailureResult(
                    "Invalid reset token",
                    new List<string> { "The reset token is invalid" });
            }

            // Check if token has expired
            if (user.PasswordResetTokenExpiry == null ||
                user.PasswordResetTokenExpiry < DateTime.UtcNow)
            {
                return ApiResponse<bool>.FailureResult(
                    "Reset token has expired",
                    new List<string> { "The reset token has expired. Please request a new one." });
            }

            // Update password
            user.PasswordHash = _passwordHasher.HashPassword(dto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true, "Password reset successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.FailureResult(
                "An error occurred while resetting your password",
                new List<string> { ex.Message });
        }
    }

    private string GeneratePasswordResetToken()
    {
        // Generate a cryptographically secure random token
        var randomBytes = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
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

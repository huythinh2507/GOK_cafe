using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GOKCafe.Application.DTOs.Auth;
using GOKCafe.Application.DTOs.Common;
using GOKCafe.Tests.Integration.Helpers;
using Xunit;

namespace GOKCafe.Tests.Integration.Controllers;

[Collection("Integration Tests")]
public class AuthControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public AuthControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region Register Tests

    [Fact]
    public async Task Register_WithValidData_ShouldReturnCreatedWithToken()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = $"testuser{Guid.NewGuid()}@example.com",
            Password = "TestPassword123!",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "+1234567890",
            Address = "123 Test St"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<AuthResponseDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Token);
        Assert.Equal(registerDto.Email.ToLower(), result.Data.User.Email);
        Assert.Equal(registerDto.FirstName, result.Data.User.FirstName);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var email = $"duplicate{Guid.NewGuid()}@example.com";
        var registerDto1 = new RegisterDto
        {
            Email = email,
            Password = "TestPassword123!",
            FirstName = "Test",
            LastName = "User"
        };

        var registerDto2 = new RegisterDto
        {
            Email = email,
            Password = "DifferentPassword456!",
            FirstName = "Another",
            LastName = "User"
        };

        // Act
        await _client.PostAsJsonAsync("/api/v1/auth/register", registerDto1);
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerDto2);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<AuthResponseDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Contains("Email already registered", result.Message);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "invalid-email",
            Password = "TestPassword123!",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkWithToken()
    {
        // Arrange - First register a user
        var email = $"logintest{Guid.NewGuid()}@example.com";
        var password = "TestPassword123!";

        var registerDto = new RegisterDto
        {
            Email = email,
            Password = password,
            FirstName = "Login",
            LastName = "Test"
        };

        await _client.PostAsJsonAsync("/api/v1/auth/register", registerDto);

        var loginDto = new LoginDto
        {
            Email = email,
            Password = password
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<AuthResponseDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Token);
        Assert.Equal(email.ToLower(), result.Data.User.Email);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var email = $"wrongpass{Guid.NewGuid()}@example.com";
        var registerDto = new RegisterDto
        {
            Email = email,
            Password = "CorrectPassword123!",
            FirstName = "Test",
            LastName = "User"
        };

        await _client.PostAsJsonAsync("/api/v1/auth/register", registerDto);

        var loginDto = new LoginDto
        {
            Email = email,
            Password = "WrongPassword456!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<AuthResponseDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Contains("Invalid credentials", result.Message);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "AnyPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Profile Tests

    [Fact]
    public async Task GetProfile_WithValidToken_ShouldReturnUserProfile()
    {
        // Arrange - Register and login to get token
        var email = $"profile{Guid.NewGuid()}@example.com";
        var registerDto = new RegisterDto
        {
            Email = email,
            Password = "TestPassword123!",
            FirstName = "Profile",
            LastName = "Test"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerDto);
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var registerResult = JsonSerializer.Deserialize<ApiResponse<AuthResponseDto>>(registerContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var token = registerResult!.Data!.Token;

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/profile");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<UserDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(email.ToLower(), result.Data.Email);
    }

    [Fact]
    public async Task GetProfile_WithoutToken_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/auth/profile");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProfile_WithValidData_ShouldUpdateProfile()
    {
        // Arrange - Register to get token
        var email = $"update{Guid.NewGuid()}@example.com";
        var registerDto = new RegisterDto
        {
            Email = email,
            Password = "TestPassword123!",
            FirstName = "Original",
            LastName = "Name"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerDto);
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var registerResult = JsonSerializer.Deserialize<ApiResponse<AuthResponseDto>>(registerContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var token = registerResult!.Data!.Token;

        var updateDto = new UpdateProfileDto
        {
            FirstName = "Updated",
            LastName = "Name",
            PhoneNumber = "+9876543210",
            Address = "456 Updated St"
        };

        // Act
        var request = new HttpRequestMessage(HttpMethod.Put, "/api/v1/auth/profile")
        {
            Content = JsonContent.Create(updateDto)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<UserDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(updateDto.FirstName, result.Data!.FirstName);
        Assert.Equal(updateDto.LastName, result.Data.LastName);
    }

    #endregion

    #region Change Password Tests

    [Fact]
    public async Task ChangePassword_WithCorrectCurrentPassword_ShouldSucceed()
    {
        // Arrange - Register to get token
        var email = $"changepass{Guid.NewGuid()}@example.com";
        var originalPassword = "OriginalPassword123!";
        var registerDto = new RegisterDto
        {
            Email = email,
            Password = originalPassword,
            FirstName = "Test",
            LastName = "User"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerDto);
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var registerResult = JsonSerializer.Deserialize<ApiResponse<AuthResponseDto>>(registerContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var token = registerResult!.Data!.Token;

        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = originalPassword,
            NewPassword = "NewPassword456!"
        };

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/change-password")
        {
            Content = JsonContent.Create(changePasswordDto)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<bool>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task ChangePassword_WithWrongCurrentPassword_ShouldFail()
    {
        // Arrange - Register to get token
        var email = $"wrongcurrent{Guid.NewGuid()}@example.com";
        var registerDto = new RegisterDto
        {
            Email = email,
            Password = "CorrectPassword123!",
            FirstName = "Test",
            LastName = "User"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerDto);
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var registerResult = JsonSerializer.Deserialize<ApiResponse<AuthResponseDto>>(registerContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var token = registerResult!.Data!.Token;

        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = "WrongCurrentPassword123!",
            NewPassword = "NewPassword456!"
        };

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/change-password")
        {
            Content = JsonContent.Create(changePasswordDto)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Deactivate Account Tests

    [Fact]
    public async Task DeactivateAccount_WithValidToken_ShouldDeactivateAccount()
    {
        // Arrange - Register to get token
        var email = $"deactivate{Guid.NewGuid()}@example.com";
        var registerDto = new RegisterDto
        {
            Email = email,
            Password = "TestPassword123!",
            FirstName = "Test",
            LastName = "User"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerDto);
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var registerResult = JsonSerializer.Deserialize<ApiResponse<AuthResponseDto>>(registerContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var token = registerResult!.Data!.Token;

        // Act
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/auth/deactivate");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<bool>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.True(result.Success);

        // Verify login fails after deactivation
        var loginDto = new LoginDto
        {
            Email = email,
            Password = "TestPassword123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
        Assert.Equal(HttpStatusCode.BadRequest, loginResponse.StatusCode);
    }

    #endregion
}

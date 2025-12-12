using GOKCafe.Application.DTOs.Auth;
using GOKCafe.Application.Services;
using GOKCafe.Domain.Entities;
using GOKCafe.Domain.Interfaces;
using GOKCafe.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace GOKCafe.Tests.Unit.Services;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockConfiguration = new Mock<IConfiguration>();

        // Setup configuration for JWT
        _mockConfiguration.Setup(c => c["Jwt:SecretKey"]).Returns("SuperSecretKeyForTestingPurposes12345678");
        _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        _mockConfiguration.Setup(c => c["Jwt:ExpirationHours"]).Returns("24");

        _authService = new AuthService(
            _mockUnitOfWork.Object,
            _mockPasswordHasher.Object,
            _mockConfiguration.Object
        );
    }

    #region RegisterAsync Tests

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldReturnSuccessWithToken()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "newuser@example.com",
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+1234567890",
            Address = "123 Main St"
        };

        var mockUserRepo = new Mock<IRepository<User>>();
        mockUserRepo.Setup(r => r.GetQueryable())
            .Returns(new List<User>().AsQueryable());

        _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _mockPasswordHasher.Setup(p => p.HashPassword(It.IsAny<string>()))
            .Returns("hashed_password");

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Token);
        Assert.Equal(registerDto.Email.ToLower(), result.Data.User.Email);
        Assert.Equal(registerDto.FirstName, result.Data.User.FirstName);
        _mockUnitOfWork.Verify(u => u.Users.AddAsync(It.IsAny<User>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldReturnFailure()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "existing@example.com",
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe"
        };

        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "existing@example.com"
        };

        var mockUserRepo = new Mock<IRepository<User>>();
        mockUserRepo.Setup(r => r.GetQueryable())
            .Returns(new List<User> { existingUser }.AsQueryable());

        _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Email already registered", result.Message);
        _mockUnitOfWork.Verify(u => u.Users.AddAsync(It.IsAny<User>()), Times.Never);
    }

    #endregion

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccessWithToken()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "user@example.com",
            Password = "CorrectPassword123!"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            PasswordHash = "hashed_password",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true,
            Role = UserRole.Customer
        };

        var mockUserRepo = new Mock<IRepository<User>>();
        mockUserRepo.Setup(r => r.GetQueryable())
            .Returns(new List<User> { user }.AsQueryable());

        _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _mockPasswordHasher.Setup(p => p.VerifyPassword(loginDto.Password, user.PasswordHash))
            .Returns(true);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Token);
        Assert.Equal(user.Email, result.Data.User.Email);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ShouldReturnFailure()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "AnyPassword123!"
        };

        var mockUserRepo = new Mock<IRepository<User>>();
        mockUserRepo.Setup(r => r.GetQueryable())
            .Returns(new List<User>().AsQueryable());

        _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid credentials", result.Message);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldReturnFailure()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "user@example.com",
            Password = "WrongPassword123!"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            PasswordHash = "hashed_password",
            IsActive = true
        };

        var mockUserRepo = new Mock<IRepository<User>>();
        mockUserRepo.Setup(r => r.GetQueryable())
            .Returns(new List<User> { user }.AsQueryable());

        _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _mockPasswordHasher.Setup(p => p.VerifyPassword(loginDto.Password, user.PasswordHash))
            .Returns(false);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid credentials", result.Message);
    }

    [Fact]
    public async Task LoginAsync_WithInactiveAccount_ShouldReturnFailure()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "inactive@example.com",
            Password = "Password123!"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "inactive@example.com",
            PasswordHash = "hashed_password",
            IsActive = false
        };

        var mockUserRepo = new Mock<IRepository<User>>();
        mockUserRepo.Setup(r => r.GetQueryable())
            .Returns(new List<User> { user }.AsQueryable());

        _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Account deactivated", result.Message);
    }

    #endregion

    #region GetProfileAsync Tests

    [Fact]
    public async Task GetProfileAsync_WithValidUserId_ShouldReturnUserProfile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+1234567890",
            Address = "123 Main St",
            Role = UserRole.Customer,
            IsActive = true
        };

        var mockUserRepo = new Mock<IRepository<User>>();
        mockUserRepo.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);

        // Act
        var result = await _authService.GetProfileAsync(userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(user.Email, result.Data.Email);
        Assert.Equal(user.FirstName, result.Data.FirstName);
    }

    [Fact]
    public async Task GetProfileAsync_WithNonExistentUserId_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var mockUserRepo = new Mock<IRepository<User>>();
        mockUserRepo.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);

        // Act
        var result = await _authService.GetProfileAsync(userId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User not found", result.Message);
    }

    #endregion

    #region UpdateProfileAsync Tests

    [Fact]
    public async Task UpdateProfileAsync_WithValidData_ShouldUpdateAndReturnProfile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+1234567890",
            IsActive = true
        };

        var updateDto = new UpdateProfileDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            PhoneNumber = "+0987654321",
            Address = "456 Oak Ave"
        };

        var mockUserRepo = new Mock<IRepository<User>>();
        mockUserRepo.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);

        // Act
        var result = await _authService.UpdateProfileAsync(userId, updateDto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(updateDto.FirstName, result.Data.FirstName);
        Assert.Equal(updateDto.LastName, result.Data.LastName);
        _mockUnitOfWork.Verify(u => u.Users.Update(It.IsAny<User>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    #endregion

    #region ChangePasswordAsync Tests

    [Fact]
    public async Task ChangePasswordAsync_WithCorrectCurrentPassword_ShouldChangePassword()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            PasswordHash = "old_hashed_password",
            IsActive = true
        };

        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword456!"
        };

        var mockUserRepo = new Mock<IRepository<User>>();
        mockUserRepo.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
        _mockPasswordHasher.Setup(p => p.VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
            .Returns(true);
        _mockPasswordHasher.Setup(p => p.HashPassword(changePasswordDto.NewPassword))
            .Returns("new_hashed_password");

        // Act
        var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);

        // Assert
        Assert.True(result.Success);
        _mockUnitOfWork.Verify(u => u.Users.Update(It.IsAny<User>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithIncorrectCurrentPassword_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            PasswordHash = "hashed_password",
            IsActive = true
        };

        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = "WrongPassword123!",
            NewPassword = "NewPassword456!"
        };

        var mockUserRepo = new Mock<IRepository<User>>();
        mockUserRepo.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);
        _mockPasswordHasher.Setup(p => p.VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
            .Returns(false);

        // Act
        var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid current password", result.Message);
        _mockUnitOfWork.Verify(u => u.Users.Update(It.IsAny<User>()), Times.Never);
    }

    #endregion

    #region DeactivateAccountAsync Tests

    [Fact]
    public async Task DeactivateAccountAsync_WithValidUser_ShouldDeactivateAccount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            IsActive = true
        };

        var mockUserRepo = new Mock<IRepository<User>>();
        mockUserRepo.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);

        // Act
        var result = await _authService.DeactivateAccountAsync(userId);

        // Assert
        Assert.True(result.Success);
        _mockUnitOfWork.Verify(u => u.Users.Update(It.Is<User>(u => u.IsActive == false)), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeactivateAccountAsync_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var mockUserRepo = new Mock<IRepository<User>>();
        mockUserRepo.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepo.Object);

        // Act
        var result = await _authService.DeactivateAccountAsync(userId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User not found", result.Message);
    }

    #endregion
}

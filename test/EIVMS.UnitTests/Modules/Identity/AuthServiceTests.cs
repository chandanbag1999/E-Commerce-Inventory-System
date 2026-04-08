using EIVMS.Application.Common.Interfaces;
using EIVMS.Application.Modules.Identity.DTOs;
using EIVMS.Application.Modules.Identity.Interfaces;
using EIVMS.Application.Modules.Identity.Services;
using EIVMS.Application.Modules.Identity.Validators;
using EIVMS.Domain.Entities.Identity;
using FluentAssertions;
using Moq;

namespace EIVMS.UnitTests.Modules.Identity;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _jwtServiceMock = new Mock<IJwtService>();
        _passwordHasherMock = new Mock<IPasswordHasher>();

        var registerValidator = new RegisterRequestValidator();
        var loginValidator = new LoginRequestValidator();

        _authService = new AuthService(
            _userRepositoryMock.Object,
            _jwtServiceMock.Object,
            _passwordHasherMock.Object,
            registerValidator,
            loginValidator);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldReturnSuccess()
    {
        var dto = new RegisterRequestDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "Password@123",
            ConfirmPassword = "Password@123"
        };

        var customerRole = Role.Create("Customer", "Customer role");

        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        _passwordHasherMock
            .Setup(h => h.HashPassword(It.IsAny<string>()))
            .Returns("hashedPassword123");

        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _userRepositoryMock
            .Setup(r => r.GetRoleByNameAsync("Customer"))
            .ReturnsAsync(customerRole);

        _userRepositoryMock
            .Setup(r => r.GetByIdWithRolesAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) =>
            {
                var user = User.Create("John", "Doe", "john@example.com", "hashedPassword");
                return user;
            });

        _jwtServiceMock.Setup(j => j.GenerateAccessToken(It.IsAny<User>(),
            It.IsAny<List<string>>(), It.IsAny<List<string>>()))
            .Returns("fake.jwt.token");

        _jwtServiceMock.Setup(j => j.GenerateRefreshToken())
            .Returns("fakeRefreshToken");

        _jwtServiceMock.Setup(j => j.HashToken(It.IsAny<string>()))
            .Returns("hashedRefreshToken");

        _jwtServiceMock.Setup(j => j.GetAccessTokenExpiry())
            .Returns(DateTime.UtcNow.AddMinutes(15));

        _jwtServiceMock.Setup(j => j.GetRefreshTokenExpiry())
            .Returns(DateTime.UtcNow.AddDays(7));

        _userRepositoryMock
            .Setup(r => r.AddRefreshTokenAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        var result = await _authService.RegisterAsync(dto);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().Be("fake.jwt.token");
        result.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ShouldReturn409()
    {
        var dto = new RegisterRequestDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "existing@example.com",
            Password = "Password@123",
            ConfirmPassword = "Password@123"
        };

        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        var result = await _authService.RegisterAsync(dto);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("already registered");
    }

    [Fact]
    public async Task RegisterAsync_WithPasswordMismatch_ShouldReturn422()
    {
        var dto = new RegisterRequestDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "Password@123",
            ConfirmPassword = "DifferentPassword@123"
        };

        var result = await _authService.RegisterAsync(dto);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(422);
        result.Errors.Should().Contain(e => e.Contains("Passwords do not match"));
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccess()
    {
        var dto = new LoginRequestDto
        {
            Email = "john@example.com",
            Password = "Password@123",
            IpAddress = "127.0.0.1",
            UserAgent = "TestAgent"
        };

        var user = User.Create("John", "Doe", "john@example.com", "hashedPassword");

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("john@example.com"))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.VerifyPassword("Password@123", "hashedPassword"))
            .Returns(true);

        _userRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _userRepositoryMock
            .Setup(r => r.GetByIdWithRolesAsync(It.IsAny<Guid>()))
            .ReturnsAsync(user);

        _jwtServiceMock.Setup(j => j.GenerateAccessToken(It.IsAny<User>(),
            It.IsAny<List<string>>(), It.IsAny<List<string>>()))
            .Returns("valid.jwt.token");

        _jwtServiceMock.Setup(j => j.GenerateRefreshToken())
            .Returns("validRefreshToken");

        _jwtServiceMock.Setup(j => j.HashToken(It.IsAny<string>()))
            .Returns("hashedRefreshToken");

        _jwtServiceMock.Setup(j => j.GetAccessTokenExpiry())
            .Returns(DateTime.UtcNow.AddMinutes(15));

        _jwtServiceMock.Setup(j => j.GetRefreshTokenExpiry())
            .Returns(DateTime.UtcNow.AddDays(7));

        _userRepositoryMock
            .Setup(r => r.AddRefreshTokenAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        var result = await _authService.LoginAsync(dto);

        result.Success.Should().BeTrue();
        result.Data!.AccessToken.Should().Be("valid.jwt.token");
        result.Message.Should().Be("Login successful");
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ShouldReturn401()
    {
        var dto = new LoginRequestDto
        {
            Email = "notexist@example.com",
            Password = "Password@123"
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var result = await _authService.LoginAsync(dto);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(401);
        result.Message.Should().Be("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldReturn401()
    {
        var dto = new LoginRequestDto
        {
            Email = "john@example.com",
            Password = "WrongPassword@123"
        };

        var user = User.Create("John", "Doe", "john@example.com", "hashedPassword");

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("john@example.com"))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.VerifyPassword("WrongPassword@123", "hashedPassword"))
            .Returns(false);

        _userRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var result = await _authService.LoginAsync(dto);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(401);
        result.Message.Should().Be("Invalid email or password");
    }
}
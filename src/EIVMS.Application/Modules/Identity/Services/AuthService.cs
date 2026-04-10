using EIVMS.Application.Common.Interfaces;
using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.Identity.DTOs;
using EIVMS.Application.Modules.Identity.Interfaces;
using EIVMS.Domain.Entities.Identity;
using FluentValidation;

namespace EIVMS.Application.Modules.Identity.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<RegisterRequestDto> _registerValidator;
    private readonly IValidator<LoginRequestDto> _loginValidator;

    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 15;

    public AuthService(
        IUserRepository userRepository,
        IJwtService jwtService,
        IPasswordHasher passwordHasher,
        IValidator<RegisterRequestDto> registerValidator,
        IValidator<LoginRequestDto> loginValidator)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto dto)
    {
        var validationResult = await _registerValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return ApiResponse<AuthResponseDto>.ErrorResponse(
                "Validation failed",
                statusCode: 422,
                errors: errors);
        }

        var emailExists = await _userRepository.EmailExistsAsync(dto.Email.ToLowerInvariant());
        if (emailExists)
        {
            return ApiResponse<AuthResponseDto>.ErrorResponse(
                "Email already registered",
                statusCode: 409);
        }

        var passwordHash = _passwordHasher.HashPassword(dto.Password);
        var user = User.Create(dto.FirstName, dto.LastName, dto.Email, passwordHash);
        await _userRepository.AddAsync(user);

        var staffRole = await _userRepository.GetRoleByNameAsync("Staff");
        if (staffRole != null)
        {
            var userRole = UserRole.Create(user.Id, staffRole.Id);
            await _userRepository.AddUserRoleAsync(userRole);
        }

        var userWithRoles = await _userRepository.GetByIdWithRolesAsync(user.Id);
        var roles = ExtractRoles(userWithRoles!);
        var permissions = ExtractPermissions(userWithRoles!);

        var accessToken = _jwtService.GenerateAccessToken(user, roles, permissions);
        var rawRefreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenHash = _jwtService.HashToken(rawRefreshToken);

        var tokenFamily = Guid.NewGuid().ToString();
        var refreshToken = RefreshToken.Create(
            userId: user.Id,
            tokenHash: refreshTokenHash,
            tokenFamily: tokenFamily,
            expiresAt: _jwtService.GetRefreshTokenExpiry(),
            ipAddress: "registration",
            userAgent: "registration");

        await _userRepository.AddRefreshTokenAsync(refreshToken);

        var response = BuildAuthResponse(
            accessToken,
            rawRefreshToken,
            userWithRoles!,
            roles,
            permissions);

        return ApiResponse<AuthResponseDto>.SuccessResponse(
            response,
            "Registration successful",
            statusCode: 201);
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto dto)
    {
        var validationResult = await _loginValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ApiResponse<AuthResponseDto>.ErrorResponse(
                "Invalid credentials",
                statusCode: 401);
        }

        var user = await _userRepository.GetByEmailAsync(dto.Email.ToLowerInvariant());

        if (user == null || !user.IsActive)
        {
            return ApiResponse<AuthResponseDto>.ErrorResponse(
                "Invalid email or password",
                statusCode: 401);
        }

        if (user.IsLockedOut())
        {
            var lockMessage = $"Account locked until {user.LockedUntil:yyyy-MM-dd HH:mm:ss} UTC";
            return ApiResponse<AuthResponseDto>.ErrorResponse(
                lockMessage,
                statusCode: 423);
        }

        var isPasswordValid = _passwordHasher.VerifyPassword(dto.Password, user.PasswordHash);

        if (!isPasswordValid)
        {
            user.IncrementFailedAttempts();

            if (user.FailedLoginAttempts >= MaxFailedAttempts)
            {
                user.LockAccount(DateTime.UtcNow.AddMinutes(LockoutMinutes));
            }

            await _userRepository.UpdateAsync(user);

            return ApiResponse<AuthResponseDto>.ErrorResponse(
                "Invalid email or password",
                statusCode: 401);
        }

        user.ResetFailedAttempts();
        user.UpdateLastLogin();
        await _userRepository.UpdateAsync(user);

        var userWithRoles = await _userRepository.GetByIdWithRolesAsync(user.Id);
        var roles = ExtractRoles(userWithRoles!);
        var permissions = ExtractPermissions(userWithRoles!);

        var accessToken = _jwtService.GenerateAccessToken(userWithRoles!, roles, permissions);
        var rawRefreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenHash = _jwtService.HashToken(rawRefreshToken);

        var tokenFamily = Guid.NewGuid().ToString();
        var refreshToken = RefreshToken.Create(
            userId: user.Id,
            tokenHash: refreshTokenHash,
            tokenFamily: tokenFamily,
            expiresAt: _jwtService.GetRefreshTokenExpiry(),
            ipAddress: dto.IpAddress ?? "unknown",
            userAgent: dto.UserAgent ?? "unknown");

        await _userRepository.AddRefreshTokenAsync(refreshToken);

        var response = BuildAuthResponse(
            accessToken,
            rawRefreshToken,
            userWithRoles!,
            roles,
            permissions);

        return ApiResponse<AuthResponseDto>.SuccessResponse(response, "Login successful");
    }

    public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RefreshToken))
        {
            return ApiResponse<AuthResponseDto>.ErrorResponse(
                "Refresh token is required",
                statusCode: 401);
        }

        var tokenHash = _jwtService.HashToken(dto.RefreshToken);
        var storedToken = await _userRepository.GetRefreshTokenAsync(tokenHash);

        if (storedToken == null)
        {
            return ApiResponse<AuthResponseDto>.ErrorResponse(
                "Invalid refresh token",
                statusCode: 401);
        }

        if (storedToken.IsRevoked)
        {
            await _userRepository.RevokeTokenFamilyAsync(storedToken.TokenFamily);

            return ApiResponse<AuthResponseDto>.ErrorResponse(
                "Token reuse detected. All sessions revoked. Please login again.",
                statusCode: 401);
        }

        if (!storedToken.IsActive)
        {
            return ApiResponse<AuthResponseDto>.ErrorResponse(
                "Refresh token has expired. Please login again.",
                statusCode: 401);
        }

        var user = await _userRepository.GetByIdWithRolesAsync(storedToken.UserId);
        if (user == null || !user.IsActive)
        {
            return ApiResponse<AuthResponseDto>.ErrorResponse(
                "User not found or inactive",
                statusCode: 401);
        }

        var roles = ExtractRoles(user);
        var permissions = ExtractPermissions(user);
        var newAccessToken = _jwtService.GenerateAccessToken(user, roles, permissions);
        var newRawRefreshToken = _jwtService.GenerateRefreshToken();
        var newRefreshTokenHash = _jwtService.HashToken(newRawRefreshToken);

        storedToken.Revoke("Rotated", newRefreshTokenHash);
        await _userRepository.UpdateRefreshTokenAsync(storedToken);

        var newRefreshToken = RefreshToken.Create(
            userId: user.Id,
            tokenHash: newRefreshTokenHash,
            tokenFamily: storedToken.TokenFamily,
            expiresAt: _jwtService.GetRefreshTokenExpiry(),
            ipAddress: storedToken.IpAddress,
            userAgent: storedToken.UserAgent);

        await _userRepository.AddRefreshTokenAsync(newRefreshToken);

        var response = BuildAuthResponse(newAccessToken, newRawRefreshToken, user, roles, permissions);
        return ApiResponse<AuthResponseDto>.SuccessResponse(response, "Token refreshed successfully");
    }

    public async Task<ApiResponse<bool>> LogoutAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return ApiResponse<bool>.ErrorResponse("Refresh token is required", statusCode: 400);
        }

        var tokenHash = _jwtService.HashToken(refreshToken);
        var storedToken = await _userRepository.GetRefreshTokenAsync(tokenHash);

        if (storedToken == null || storedToken.IsRevoked)
        {
            return ApiResponse<bool>.SuccessResponse(true, "Logged out successfully");
        }

        storedToken.Revoke("LoggedOut");
        await _userRepository.UpdateRefreshTokenAsync(storedToken);

        return ApiResponse<bool>.SuccessResponse(true, "Logged out successfully");
    }

    public async Task<ApiResponse<CurrentUserDto>> GetCurrentUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(userId);

        if (user == null)
        {
            return ApiResponse<CurrentUserDto>.ErrorResponse("User not found", statusCode: 404);
        }

        var roles = ExtractRoles(user);
        var permissions = ExtractPermissions(user);

        var currentUser = new CurrentUserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Roles = roles,
            Permissions = permissions,
            LastLoginAt = user.LastLoginAt
        };

        return ApiResponse<CurrentUserDto>.SuccessResponse(currentUser);
    }

    private static List<string> ExtractRoles(User user)
    {
        return user.UserRoles
            .Select(ur => ur.Role?.Name)
            .Where(name => name != null)
            .Select(name => name!)
            .ToList();
    }

    private static List<string> ExtractPermissions(User user)
    {
        return user.UserRoles
            .SelectMany(ur => ur.Role?.RolePermissions ?? new List<RolePermission>())
            .Select(rp => rp.Permission?.Name)
            .Where(name => name != null)
            .Select(name => name!)
            .Distinct()
            .ToList();
    }

    private AuthResponseDto BuildAuthResponse(
        string accessToken,
        string rawRefreshToken,
        User user,
        List<string> roles,
        List<string> permissions)
    {
        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            AccessTokenExpiresAt = _jwtService.GetAccessTokenExpiry(),
            RefreshTokenExpiresAt = _jwtService.GetRefreshTokenExpiry(),
            User = new CurrentUserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Roles = roles,
                Permissions = permissions,
                Role = roles.FirstOrDefault() ?? "Staff",
                LastLoginAt = user.LastLoginAt
            }
        };
    }
}
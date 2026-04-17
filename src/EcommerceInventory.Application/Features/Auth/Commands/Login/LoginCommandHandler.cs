using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Settings;
using EcommerceInventory.Application.Features.Auth.DTOs;
using EcommerceInventory.Domain.Enums;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EcommerceInventory.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler
    : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IUnitOfWork      _uow;
    private readonly ITokenService    _tokenService;
    private readonly IDateTimeService _dateTime;
    private readonly JwtSettings      _jwtSettings;

    public LoginCommandHandler(IUnitOfWork uow,
                                ITokenService tokenService,
                                IDateTimeService dateTime,
                                IOptions<JwtSettings> jwtSettings)
    {
        _uow          = uow;
        _tokenService = tokenService;
        _dateTime     = dateTime;
        _jwtSettings  = jwtSettings.Value;
    }

    public async Task<LoginResponseDto> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _uow.Users.Query()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(
                u => u.Email == request.Email.ToLower().Trim(),
                cancellationToken);

        if (user == null || user.IsDeleted)
            throw new UnauthorizedException("Invalid email or password.");

        if (user.Status == UserStatus.Inactive)
            throw new UnauthorizedException("Your account is inactive. Please contact administrator.");

        if (user.Status == UserStatus.Suspended)
            throw new UnauthorizedException("Your account has been suspended. Please contact administrator.");

        var passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!passwordValid)
            throw new UnauthorizedException("Invalid email or password.");

        var roles = user.UserRoles.Select(ur => ur.Role.Name).Distinct().ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct().ToList();

        var accessToken  = _tokenService.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var refreshTokenEntity = Domain.Entities.RefreshToken.Create(
            userId:     user.Id,
            token:      refreshToken,
            expiresAt:  _dateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpDays),
            deviceInfo: request.DeviceInfo);

        await _uow.RefreshTokens.AddAsync(refreshTokenEntity, cancellationToken);
        user.RecordLogin();
        await _uow.SaveChangesAsync(cancellationToken);

        return new LoginResponseDto
        {
            AccessToken  = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn    = _jwtSettings.AccessTokenExpMinutes * 60,
            TokenType    = "Bearer",
            User = new UserInfoDto
            {
                Id              = user.Id,
                FullName        = user.FullName,
                Email           = user.Email,
                Phone           = user.Phone,
                ProfileImageUrl = user.ProfileImageUrl,
                Status          = user.Status.ToString(),
                IsEmailVerified = user.IsEmailVerified,
                Roles           = roles,
                Permissions     = permissions,
                LastLoginAt     = user.LastLoginAt,
                CreatedAt       = user.CreatedAt
            }
        };
    }
}

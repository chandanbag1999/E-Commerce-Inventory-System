using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Settings;
using EcommerceInventory.Application.Features.Auth.DTOs;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EcommerceInventory.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, TokenDto>
{
    private readonly IUnitOfWork      _uow;
    private readonly ITokenService    _tokenService;
    private readonly IDateTimeService _dateTime;
    private readonly JwtSettings      _jwtSettings;

    public RefreshTokenCommandHandler(IUnitOfWork uow,
                                       ITokenService tokenService,
                                       IDateTimeService dateTime,
                                       IOptions<JwtSettings> jwtSettings)
    {
        _uow          = uow;
        _tokenService = tokenService;
        _dateTime     = dateTime;
        _jwtSettings  = jwtSettings.Value;
    }

    public async Task<TokenDto> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
            throw new UnauthorizedException("Invalid access token.");

        var userIdStr = principal.FindFirst("userId")?.Value
                     ?? principal.FindFirst("sub")?.Value;

        if (!Guid.TryParse(userIdStr, out var userId))
            throw new UnauthorizedException("Invalid access token.");

        var storedToken = await _uow.RefreshTokens.Query()
            .FirstOrDefaultAsync(
                rt => rt.Token == request.RefreshToken && rt.UserId == userId,
                cancellationToken);

        if (storedToken == null)
            throw new UnauthorizedException("Invalid refresh token.");

        if (storedToken.IsRevoked)
        {
            var allTokens = await _uow.RefreshTokens.Query()
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync(cancellationToken);
            foreach (var token in allTokens)
                token.Revoke();
            await _uow.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException("Security alert: refresh token reuse detected. All sessions terminated.");
        }

        if (storedToken.IsExpired)
            throw new UnauthorizedException("Refresh token has expired. Please login again.");

        var user = await _uow.Users.Query()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null || user.IsDeleted)
            throw new UnauthorizedException("User not found.");

        var roles = user.UserRoles.Select(ur => ur.Role.Name).Distinct().ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct().ToList();

        var newAccessToken  = _tokenService.GenerateAccessToken(user, roles, permissions);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        storedToken.Revoke(replacedBy: newRefreshToken);

        var newTokenEntity = Domain.Entities.RefreshToken.Create(
            userId:     userId,
            token:      newRefreshToken,
            expiresAt:  _dateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpDays),
            deviceInfo: storedToken.DeviceInfo);

        await _uow.RefreshTokens.AddAsync(newTokenEntity, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new TokenDto
        {
            AccessToken  = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn    = _jwtSettings.AccessTokenExpMinutes * 60,
            TokenType    = "Bearer"
        };
    }
}

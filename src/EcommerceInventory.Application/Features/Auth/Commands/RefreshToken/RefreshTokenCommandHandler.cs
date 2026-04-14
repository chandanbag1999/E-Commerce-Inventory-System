using System.Security.Claims;
using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Auth.DTOs;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<LoginResponseDto>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Domain.Entities.RefreshToken> _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(
        IRepository<User> userRepository,
        IRepository<Domain.Entities.RefreshToken> refreshTokenRepository,
        IUnitOfWork unitOfWork,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<Result<LoginResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Extract principal from expired access token
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            throw new UnauthorizedException("Invalid access token");
        }

        var userId = Guid.Parse(userIdClaim.Value);

        // Find the refresh token in database
        var storedToken = await _refreshTokenRepository.Query()
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.UserId == userId, cancellationToken);

        if (storedToken == null)
        {
            throw new UnauthorizedException("Invalid refresh token");
        }

        // Check for reuse attack
        if (storedToken.IsRevoked)
        {
            // Revoke ALL tokens for this user (security measure)
            await RevokeAllUserRefreshTokensAsync(userId, cancellationToken);
            throw new UnauthorizedException("Security alert: session terminated. Please login again.");
        }

        // Check expiry
        if (storedToken.ExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedException("Refresh token expired. Please login again.");
        }

        // Revoke old token (rotation)
        var newRefreshToken = _tokenService.GenerateRefreshToken(userId);
        storedToken.Revoke(newRefreshToken.Token);

        // Re-fetch user with fresh roles/permissions
        var user = await _userRepository.Query()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedException("User not found");
        }

        // Generate new tokens
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToList();

        var newAccessToken = _tokenService.GenerateAccessToken(user, roles, permissions);

        // Save new refresh token
        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new LoginResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresIn = 900,
            TokenType = "Bearer",
            User = new UserInfoDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Roles = roles,
                Permissions = permissions,
                IsEmailVerified = user.IsEmailVerified
            }
        };

        return Result<LoginResponseDto>.SuccessResult(response);
    }

    private async Task RevokeAllUserRefreshTokensAsync(Guid userId, CancellationToken cancellationToken)
    {
        var userTokens = await _refreshTokenRepository.Query()
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in userTokens)
        {
            token.Revoke();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

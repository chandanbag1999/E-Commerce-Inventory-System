using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Auth.DTOs;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Enums;
using EcommerceInventory.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Domain.Entities.RefreshToken> _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(
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

    public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Find user with roles and permissions
        var user = await _userRepository.Query()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedException("Invalid credentials");
        }

        // Check user status
        if (user.Status == UserStatus.Inactive || user.Status == UserStatus.Suspended)
        {
            throw new UnauthorizedException("Account is disabled. Please contact support.");
        }

        if (user.DeletedAt.HasValue)
        {
            throw new UnauthorizedException("Invalid credentials");
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid credentials");
        }

        // Extract roles and permissions
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToList();

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _tokenService.GenerateRefreshToken(user.Id, request.DeviceInfo);

        // Save refresh token
        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Record login
        user.RecordLogin();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresIn = 900, // 15 minutes
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
}

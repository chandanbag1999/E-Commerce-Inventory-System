using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.Auth.DTOs;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Auth.Queries.GetMe;

public class GetMeQueryHandler : IRequestHandler<GetMeQuery, UserInfoDto>
{
    private readonly IUnitOfWork _uow;

    public GetMeQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<UserInfoDto> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var user = await _uow.Users.Query()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null || user.IsDeleted)
            throw new NotFoundException("User not found.");

        var roles = user.UserRoles.Select(ur => ur.Role.Name).Distinct().ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct().ToList();

        return new UserInfoDto
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
        };
    }
}

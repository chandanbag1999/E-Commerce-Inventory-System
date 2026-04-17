using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.Users.DTOs;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler
    : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUnitOfWork _uow;

    public GetUserByIdQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<UserDto> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _uow.Users.Query()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null || user.IsDeleted)
            throw new NotFoundException("User", request.Id);

        return new UserDto
        {
            Id              = user.Id,
            FullName        = user.FullName,
            Email           = user.Email,
            Phone           = user.Phone,
            ProfileImageUrl = user.ProfileImageUrl,
            Status          = user.Status.ToString(),
            IsEmailVerified = user.IsEmailVerified,
            Roles           = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
            LastLoginAt     = user.LastLoginAt,
            CreatedAt       = user.CreatedAt,
            UpdatedAt       = user.UpdatedAt
        };
    }
}

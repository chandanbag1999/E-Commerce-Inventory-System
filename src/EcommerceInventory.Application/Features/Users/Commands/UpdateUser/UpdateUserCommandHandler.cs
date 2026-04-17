using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.Users.DTOs;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler
    : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly IUnitOfWork _uow;

    public UpdateUserCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<UserDto> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _uow.Users.Query()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null || user.IsDeleted)
            throw new NotFoundException("User", request.Id);

        user.UpdateProfile(request.FullName, request.Phone);
        await _uow.SaveChangesAsync(cancellationToken);

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

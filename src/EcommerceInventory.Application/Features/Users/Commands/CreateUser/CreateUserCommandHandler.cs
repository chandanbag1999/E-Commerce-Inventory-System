using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.Users.DTOs;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler
    : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IUnitOfWork         _uow;
    private readonly IUserRoleRepository _userRoleRepo;
    private readonly IEmailService       _emailService;

    public CreateUserCommandHandler(IUnitOfWork uow,
                                     IUserRoleRepository userRoleRepo,
                                     IEmailService emailService)
    {
        _uow          = uow;
        _userRoleRepo = userRoleRepo;
        _emailService = emailService;
    }

    public async Task<UserDto> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        var emailExists = await _uow.Users.Query()
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == request.Email.ToLower().Trim(),
                      cancellationToken);

        if (emailExists)
            throw new DomainException("An account with this email already exists.");

        if (request.RoleId.HasValue)
        {
            var roleExists = await _uow.Roles.ExistsAsync(
                request.RoleId.Value, cancellationToken);
            if (!roleExists)
                throw new NotFoundException("Role", request.RoleId.Value);
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = User.Create(request.FullName, request.Email,
                               passwordHash, request.Phone);

        await _uow.Users.AddAsync(user, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        List<string> roles = new();

        if (request.RoleId.HasValue)
        {
            var userRole = new UserRole
            {
                UserId     = user.Id,
                RoleId     = request.RoleId.Value,
                AssignedAt = DateTime.UtcNow
            };
            await _userRoleRepo.AddAsync(userRole, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            var role = await _uow.Roles.GetByIdAsync(
                request.RoleId.Value, cancellationToken);
            if (role != null) roles.Add(role.Name);
        }

        _ = Task.Run(async () =>
        {
            try { await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName); }
            catch { }
        }, CancellationToken.None);

        return new UserDto
        {
            Id              = user.Id,
            FullName        = user.FullName,
            Email           = user.Email,
            Phone           = user.Phone,
            ProfileImageUrl = user.ProfileImageUrl,
            Status          = user.Status.ToString(),
            IsEmailVerified = user.IsEmailVerified,
            Roles           = roles,
            CreatedAt       = user.CreatedAt,
            UpdatedAt       = user.UpdatedAt
        };
    }
}

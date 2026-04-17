using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Exceptions;
using MediatR;

namespace EcommerceInventory.Application.Features.Users.Commands.AssignRole;

public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, bool>
{
    private readonly IUnitOfWork         _uow;
    private readonly IUserRoleRepository _userRoleRepo;
    private readonly ICurrentUserService _currentUser;

    public AssignRoleCommandHandler(IUnitOfWork uow,
                                     IUserRoleRepository userRoleRepo,
                                     ICurrentUserService currentUser)
    {
        _uow          = uow;
        _userRoleRepo = userRoleRepo;
        _currentUser  = currentUser;
    }

    public async Task<bool> Handle(
        AssignRoleCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null || user.IsDeleted)
            throw new NotFoundException("User", request.UserId);

        var role = await _uow.Roles.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
            throw new NotFoundException("Role", request.RoleId);

        var alreadyAssigned = await _userRoleRepo.ExistsAsync(
            request.UserId, request.RoleId, cancellationToken);

        if (alreadyAssigned)
            throw new DomainException("User already has this role.");

        var userRole = new UserRole
        {
            UserId     = request.UserId,
            RoleId     = request.RoleId,
            AssignedAt = DateTime.UtcNow,
            AssignedBy = _currentUser.UserId
        };

        await _userRoleRepo.AddAsync(userRole, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

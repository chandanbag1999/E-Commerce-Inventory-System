using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Domain.Exceptions;
using MediatR;

namespace EcommerceInventory.Application.Features.Users.Commands.RevokeRole;

public class RevokeRoleCommandHandler : IRequestHandler<RevokeRoleCommand, bool>
{
    private readonly IUnitOfWork         _uow;
    private readonly IUserRoleRepository _userRoleRepo;

    public RevokeRoleCommandHandler(IUnitOfWork uow,
                                     IUserRoleRepository userRoleRepo)
    {
        _uow          = uow;
        _userRoleRepo = userRoleRepo;
    }

    public async Task<bool> Handle(
        RevokeRoleCommand request,
        CancellationToken cancellationToken)
    {
        var userRole = await _userRoleRepo.FindAsync(
            request.UserId, request.RoleId, cancellationToken);

        if (userRole == null)
            throw new NotFoundException("User role assignment not found.");

        _userRoleRepo.Remove(userRole);
        await _uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

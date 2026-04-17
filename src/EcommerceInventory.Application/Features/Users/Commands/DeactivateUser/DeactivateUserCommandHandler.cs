using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Domain.Exceptions;
using MediatR;

namespace EcommerceInventory.Application.Features.Users.Commands.DeactivateUser;

public class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public DeactivateUserCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(
        DeactivateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByIdAsync(request.Id, cancellationToken);
        if (user == null || user.IsDeleted)
            throw new NotFoundException("User", request.Id);

        user.Deactivate();
        await _uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

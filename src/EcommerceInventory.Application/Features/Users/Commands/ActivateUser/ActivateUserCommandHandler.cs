using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Domain.Exceptions;
using MediatR;

namespace EcommerceInventory.Application.Features.Users.Commands.ActivateUser;

public class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public ActivateUserCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(
        ActivateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByIdAsync(request.Id, cancellationToken);
        if (user == null || user.IsDeleted)
            throw new NotFoundException("User", request.Id);

        user.Activate();
        await _uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

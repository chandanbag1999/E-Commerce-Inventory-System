using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Domain.Exceptions;
using MediatR;

namespace EcommerceInventory.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public DeleteUserCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(
        DeleteUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByIdAsync(request.Id, cancellationToken);
        if (user == null || user.IsDeleted)
            throw new NotFoundException("User", request.Id);

        user.SoftDelete();
        await _uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

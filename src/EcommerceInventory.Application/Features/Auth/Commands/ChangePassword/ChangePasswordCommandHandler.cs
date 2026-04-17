using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Domain.Exceptions;
using MediatR;

namespace EcommerceInventory.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public ChangePasswordCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null || user.IsDeleted)
            throw new NotFoundException("User not found.");

        var isValid = BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash);
        if (!isValid)
            throw new UnauthorizedException("Current password is incorrect.");

        var newHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.ChangePassword(newHash);
        await _uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

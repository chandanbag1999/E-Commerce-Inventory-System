using EcommerceInventory.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public LogoutCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var token = await _uow.RefreshTokens.Query()
            .FirstOrDefaultAsync(
                rt => rt.Token == request.RefreshToken && rt.UserId == request.UserId,
                cancellationToken);

        if (token == null || token.IsRevoked)
            return true;

        token.Revoke();
        await _uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

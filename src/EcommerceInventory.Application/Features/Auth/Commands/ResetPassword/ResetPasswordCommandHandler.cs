using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public ResetPasswordCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var tokens = await _uow.PasswordResetTokens.Query()
            .Include(t => t.User)
            .Where(t => !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        Domain.Entities.PasswordResetToken? matchedToken = null;
        foreach (var t in tokens)
        {
            try
            {
                if (BCrypt.Net.BCrypt.Verify(request.Token, t.TokenHash))
                {
                    matchedToken = t;
                    break;
                }
            }
            catch { }
        }

        if (matchedToken == null)
            throw new DomainException("Invalid or expired reset token.");

        var newHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        matchedToken.User.ChangePassword(newHash);
        matchedToken.MarkUsed();

        var refreshTokens = await _uow.RefreshTokens.Query()
            .Where(rt => rt.UserId == matchedToken.UserId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);
        foreach (var rt in refreshTokens)
            rt.Revoke();

        await _uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

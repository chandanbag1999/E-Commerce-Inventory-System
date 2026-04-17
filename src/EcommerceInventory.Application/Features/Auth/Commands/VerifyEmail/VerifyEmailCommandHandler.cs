using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Auth.Commands.VerifyEmail;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public VerifyEmailCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            throw new DomainException("Verification token is required.");

        var tokens = await _uow.EmailVerificationTokens.Query()
            .Include(t => t.User)
            .Where(t => !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        Domain.Entities.EmailVerificationToken? matchedToken = null;
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
            throw new DomainException("Invalid or expired verification token.");

        matchedToken.MarkUsed();
        matchedToken.User.VerifyEmail();
        await _uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

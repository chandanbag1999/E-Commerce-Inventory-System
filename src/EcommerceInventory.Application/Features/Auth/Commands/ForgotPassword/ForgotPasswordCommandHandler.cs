using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly IUnitOfWork      _uow;
    private readonly IEmailService    _emailService;
    private readonly IDateTimeService _dateTime;

    public ForgotPasswordCommandHandler(IUnitOfWork uow,
                                         IEmailService emailService,
                                         IDateTimeService dateTime)
    {
        _uow          = uow;
        _emailService = emailService;
        _dateTime     = dateTime;
    }

    public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _uow.Users.Query()
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower().Trim(), cancellationToken);

        if (user == null || user.IsDeleted)
            return true;

        var oldTokens = await _uow.PasswordResetTokens.Query()
            .Where(t => t.UserId == user.Id && !t.IsUsed)
            .ToListAsync(cancellationToken);
        foreach (var old in oldTokens)
            old.MarkUsed();

        var rawToken  = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var tokenHash = BCrypt.Net.BCrypt.HashPassword(rawToken);
        var expiresAt = _dateTime.UtcNow.AddHours(1);

        var resetToken = PasswordResetToken.Create(user.Id, tokenHash, expiresAt);
        await _uow.PasswordResetTokens.AddAsync(resetToken, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        _ = Task.Run(async () =>
        {
            try { await _emailService.SendPasswordResetAsync(user.Email, user.FullName, rawToken); }
            catch { }
        }, CancellationToken.None);

        return true;
    }
}

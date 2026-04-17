using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.Auth.DTOs;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler
    : IRequestHandler<RegisterCommand, RegisterResponseDto>
{
    private readonly IUnitOfWork      _uow;
    private readonly IEmailService    _emailService;
    private readonly IDateTimeService _dateTime;

    public RegisterCommandHandler(IUnitOfWork uow,
                                   IEmailService emailService,
                                   IDateTimeService dateTime)
    {
        _uow          = uow;
        _emailService = emailService;
        _dateTime     = dateTime;
    }

    public async Task<RegisterResponseDto> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var emailExists = await _uow.Users.Query()
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == request.Email.ToLower().Trim(),
                      cancellationToken);

        if (emailExists)
            throw new DomainException(
                "An account with this email already exists.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = User.Create(
            fullName:     request.FullName,
            email:        request.Email,
            passwordHash: passwordHash,
            phone:        request.Phone);

        await _uow.Users.AddAsync(user, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        var rawToken  = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var tokenHash = BCrypt.Net.BCrypt.HashPassword(rawToken);
        var expiresAt = _dateTime.UtcNow.AddHours(24);

        var verificationToken = EmailVerificationToken.Create(
            userId:    user.Id,
            tokenHash: tokenHash,
            expiresAt: expiresAt);

        await _uow.EmailVerificationTokens.AddAsync(verificationToken, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        _ = Task.Run(async () =>
        {
            try { await _emailService.SendEmailVerificationAsync(user.Email, user.FullName, rawToken); }
            catch { }
        }, CancellationToken.None);

        return new RegisterResponseDto
        {
            Id              = user.Id,
            FullName        = user.FullName,
            Email           = user.Email,
            Phone           = user.Phone,
            Status          = user.Status.ToString(),
            IsEmailVerified = user.IsEmailVerified,
            CreatedAt       = user.CreatedAt
        };
    }
}

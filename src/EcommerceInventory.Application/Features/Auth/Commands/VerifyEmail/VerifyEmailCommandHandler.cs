using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace EcommerceInventory.Application.Features.Auth.Commands.VerifyEmail;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<EmailVerificationToken> _verificationTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyEmailCommandHandler(
        IRepository<User> userRepository,
        IRepository<EmailVerificationToken> verificationTokenRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _verificationTokenRepository = verificationTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.Query()
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        if (user.IsEmailVerified)
        {
            return Result.FailureResult("Email is already verified");
        }

        // Find verification token
        var tokenHash = HashToken(request.Token);
        var storedToken = await _verificationTokenRepository.Query()
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && t.UserId == user.Id && !t.IsUsed, cancellationToken);

        if (storedToken == null)
        {
            throw new UnauthorizedException("Invalid or expired verification token");
        }

        if (storedToken.IsExpired())
        {
            throw new UnauthorizedException("Verification token has expired");
        }

        // Mark user as verified
        user.IsEmailVerified = true;
        storedToken.MarkAsUsed();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.SuccessResult("Email verified successfully");
    }

    private static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hashedBytes);
    }
}

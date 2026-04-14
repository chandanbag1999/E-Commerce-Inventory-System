using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace EcommerceInventory.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<PasswordResetToken> _resetTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordCommandHandler(
        IRepository<User> userRepository,
        IRepository<PasswordResetToken> resetTokenRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _resetTokenRepository = resetTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.Query()
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        var tokenHash = HashToken(request.Token);
        var resetToken = await _resetTokenRepository.Query()
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && t.UserId == user.Id && !t.IsUsed, cancellationToken);

        if (resetToken == null)
        {
            throw new UnauthorizedException("Invalid or expired reset token");
        }

        if (resetToken.IsExpired())
        {
            throw new UnauthorizedException("Reset token has expired");
        }

        // Update password
        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatePassword(newPasswordHash);
        resetToken.MarkAsUsed();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.SuccessResult("Password reset successful");
    }

    private static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hashedBytes);
    }
}

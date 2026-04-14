using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IRepository<Domain.Entities.RefreshToken> _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(
        IRepository<Domain.Entities.RefreshToken> refreshTokenRepository,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var token = await _refreshTokenRepository.Query()
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.UserId == request.UserId, cancellationToken);

        if (token == null)
        {
            throw new UnauthorizedException("Invalid refresh token");
        }

        token.Revoke();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.SuccessResult("Logout successful");
    }
}

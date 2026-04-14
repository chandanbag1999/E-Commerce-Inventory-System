using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Users.Commands.DeactivateUser;

public record DeactivateUserCommand : IRequest<Result<object>>
{
    public Guid UserId { get; set; }
}

public class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, Result<object>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<User> _userRepository;

    public DeactivateUserCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<User> userRepository)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
    }

    public async Task<Result<object>> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user == null)
        {
            throw new NotFoundException("User", request.UserId);
        }

        user.Deactivate();
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<object>.SuccessResult(new { }, "User deactivated successfully");
    }
}

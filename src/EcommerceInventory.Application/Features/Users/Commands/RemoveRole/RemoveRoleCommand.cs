using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Users.Commands.RemoveRole;

public record RemoveRoleCommand : IRequest<Result<object>>
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}

public class RemoveRoleCommandHandler : IRequestHandler<RemoveRoleCommand, Result<object>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<User> _userRepository;

    public RemoveRoleCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<User> userRepository)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
    }

    public async Task<Result<object>> Handle(RemoveRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.Query()
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        
        if (user == null)
        {
            throw new NotFoundException("User", request.UserId);
        }

        var userRole = user.UserRoles.FirstOrDefault(ur => ur.RoleId == request.RoleId);
        if (userRole == null)
        {
            throw new BusinessRuleViolationException("Role is not assigned to this user");
        }

        user.UserRoles.Remove(userRole);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<object>.SuccessResult(new { }, "Role removed successfully");
    }
}

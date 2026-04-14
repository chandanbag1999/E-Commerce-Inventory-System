using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Users.Commands.AssignRole;

public record AssignRoleCommand : IRequest<Result<object>>
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}

public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, Result<object>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Role> _roleRepository;

    public AssignRoleCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<User> userRepository,
        IRepository<Role> roleRepository)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task<Result<object>> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User", request.UserId);
        }

        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            throw new NotFoundException("Role", request.RoleId);
        }

        // Check if role already assigned
        var existingRole = user.UserRoles.FirstOrDefault(ur => ur.RoleId == request.RoleId);
        if (existingRole != null)
        {
            throw new BusinessRuleViolationException("Role is already assigned to this user");
        }

        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            AssignedAt = DateTime.UtcNow
        };

        user.UserRoles.Add(userRole);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<object>.SuccessResult(new { }, "Role assigned successfully");
    }
}

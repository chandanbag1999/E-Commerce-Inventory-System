using MediatR;

namespace EcommerceInventory.Application.Features.Users.Commands.AssignRole;

public class AssignRoleCommand : IRequest<bool>
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}

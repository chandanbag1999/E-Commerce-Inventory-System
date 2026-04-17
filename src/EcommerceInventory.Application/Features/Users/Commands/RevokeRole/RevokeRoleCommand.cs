using MediatR;

namespace EcommerceInventory.Application.Features.Users.Commands.RevokeRole;

public class RevokeRoleCommand : IRequest<bool>
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}

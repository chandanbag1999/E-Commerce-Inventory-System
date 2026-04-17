using MediatR;

namespace EcommerceInventory.Application.Features.Users.Commands.DeactivateUser;

public class DeactivateUserCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

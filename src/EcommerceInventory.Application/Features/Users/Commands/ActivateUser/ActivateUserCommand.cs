using MediatR;

namespace EcommerceInventory.Application.Features.Users.Commands.ActivateUser;

public class ActivateUserCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

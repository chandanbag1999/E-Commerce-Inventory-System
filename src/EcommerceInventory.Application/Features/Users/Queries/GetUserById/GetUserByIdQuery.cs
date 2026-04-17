using EcommerceInventory.Application.Features.Users.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Users.Queries.GetUserById;

public class GetUserByIdQuery : IRequest<UserDto>
{
    public Guid Id { get; set; }
}

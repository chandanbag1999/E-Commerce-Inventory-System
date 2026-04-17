using EcommerceInventory.Application.Features.Users.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommand : IRequest<UserDto>
{
    public Guid    Id       { get; set; }
    public string  FullName { get; set; } = string.Empty;
    public string? Phone    { get; set; }
}

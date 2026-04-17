using EcommerceInventory.Application.Features.Users.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommand : IRequest<UserDto>
{
    public string  FullName { get; set; } = string.Empty;
    public string  Email    { get; set; } = string.Empty;
    public string  Password { get; set; } = string.Empty;
    public string? Phone    { get; set; }
    public Guid?   RoleId   { get; set; }
}

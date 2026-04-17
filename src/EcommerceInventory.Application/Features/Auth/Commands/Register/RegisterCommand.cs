using EcommerceInventory.Application.Features.Auth.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Auth.Commands.Register;

public class RegisterCommand : IRequest<RegisterResponseDto>
{
    public string  FullName { get; set; } = string.Empty;
    public string  Email    { get; set; } = string.Empty;
    public string  Password { get; set; } = string.Empty;
    public string? Phone    { get; set; }
}
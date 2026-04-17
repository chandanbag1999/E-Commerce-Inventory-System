using EcommerceInventory.Application.Features.Auth.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Auth.Commands.Login;

public class LoginCommand : IRequest<LoginResponseDto>
{
    public string  Email      { get; set; } = string.Empty;
    public string  Password   { get; set; } = string.Empty;
    public string? DeviceInfo { get; set; }
}
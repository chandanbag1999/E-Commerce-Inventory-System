using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Auth.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Auth.Commands.Login;

public record LoginCommand : IRequest<Result<LoginResponseDto>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? DeviceInfo { get; set; }
}

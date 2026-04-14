using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Auth.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand : IRequest<Result<LoginResponseDto>>
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

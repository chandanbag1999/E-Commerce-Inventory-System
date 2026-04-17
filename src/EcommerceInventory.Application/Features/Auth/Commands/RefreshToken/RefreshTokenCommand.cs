using EcommerceInventory.Application.Features.Auth.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommand : IRequest<TokenDto>
{
    public string AccessToken  { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
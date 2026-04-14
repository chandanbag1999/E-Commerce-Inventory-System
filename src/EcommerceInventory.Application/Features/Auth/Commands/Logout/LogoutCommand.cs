using EcommerceInventory.Application.Common.Models;
using MediatR;

namespace EcommerceInventory.Application.Features.Auth.Commands.Logout;

public record LogoutCommand : IRequest<Result>
{
    public string RefreshToken { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}

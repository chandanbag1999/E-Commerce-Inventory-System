using MediatR;

namespace EcommerceInventory.Application.Features.Auth.Commands.Logout;

public class LogoutCommand : IRequest<bool>
{
    public string RefreshToken { get; set; } = string.Empty;
    public Guid   UserId       { get; set; }
}
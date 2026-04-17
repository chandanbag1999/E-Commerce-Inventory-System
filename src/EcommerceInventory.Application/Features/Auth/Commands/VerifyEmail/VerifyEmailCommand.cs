using MediatR;

namespace EcommerceInventory.Application.Features.Auth.Commands.VerifyEmail;

public class VerifyEmailCommand : IRequest<bool>
{
    public string Token { get; set; } = string.Empty;
}
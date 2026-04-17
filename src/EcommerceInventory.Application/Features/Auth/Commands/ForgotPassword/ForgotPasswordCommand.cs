using MediatR;

namespace EcommerceInventory.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommand : IRequest<bool>
{
    public string Email { get; set; } = string.Empty;
}
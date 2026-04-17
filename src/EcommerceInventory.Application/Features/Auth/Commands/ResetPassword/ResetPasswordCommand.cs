using MediatR;

namespace EcommerceInventory.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommand : IRequest<bool>
{
    public string Token       { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
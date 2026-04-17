using MediatR;

namespace EcommerceInventory.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommand : IRequest<bool>
{
    public Guid   UserId      { get; set; }
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
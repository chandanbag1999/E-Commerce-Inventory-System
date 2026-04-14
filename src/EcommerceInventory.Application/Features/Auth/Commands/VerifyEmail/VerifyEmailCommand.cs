using EcommerceInventory.Application.Common.Models;
using MediatR;

namespace EcommerceInventory.Application.Features.Auth.Commands.VerifyEmail;

public record VerifyEmailCommand : IRequest<Result>
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

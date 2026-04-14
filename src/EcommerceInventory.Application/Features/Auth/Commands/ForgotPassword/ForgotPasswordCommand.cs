using EcommerceInventory.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace EcommerceInventory.Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand : IRequest<Result>
{
    public string Email { get; set; } = string.Empty;
}

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
    }
}

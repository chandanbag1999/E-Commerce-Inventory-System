using FluentValidation;

namespace EcommerceInventory.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandValidator
    : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Reset token is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(100).WithMessage("Password max 100 characters.")
            .Matches(@"[A-Z]").WithMessage("Must contain uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Must contain lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Must contain a digit.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Must contain a special character.");
    }
}
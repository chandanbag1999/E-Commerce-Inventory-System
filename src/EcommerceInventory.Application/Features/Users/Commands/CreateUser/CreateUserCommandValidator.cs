using FluentValidation;

namespace EcommerceInventory.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(150);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(200);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Min 8 characters.")
            .Matches(@"[A-Z]").WithMessage("Must contain uppercase.")
            .Matches(@"[a-z]").WithMessage("Must contain lowercase.")
            .Matches(@"[0-9]").WithMessage("Must contain digit.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Must contain special character.");

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .When(x => x.Phone != null);
    }
}

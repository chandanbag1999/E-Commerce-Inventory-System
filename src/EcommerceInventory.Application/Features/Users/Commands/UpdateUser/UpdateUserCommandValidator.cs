using EcommerceInventory.Application.Features.Users.Commands.UpdateUser;
using FluentValidation;

namespace EcommerceInventory.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(150).WithMessage("Full name must not exceed 150 characters");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters")
            .Matches(@"^[\+]?[(]?[0-9]{1,4}[)]?[-\s\./0-9]*$").When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Invalid phone number format");
    }
}

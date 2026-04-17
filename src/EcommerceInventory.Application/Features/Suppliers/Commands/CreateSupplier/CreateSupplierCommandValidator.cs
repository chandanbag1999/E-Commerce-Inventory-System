using FluentValidation;

namespace EcommerceInventory.Application.Features.Suppliers.Commands.CreateSupplier;

public class CreateSupplierCommandValidator
    : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Supplier name is required.")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.GstNumber)
            .MaximumLength(50).WithMessage("GST number cannot exceed 50 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.GstNumber));
    }
}

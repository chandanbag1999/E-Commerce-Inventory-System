using FluentValidation;

namespace EcommerceInventory.Application.Features.Suppliers.Commands.UpdateSupplier;

public class UpdateSupplierCommandValidator
    : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Supplier ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Supplier name is required.")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }
}

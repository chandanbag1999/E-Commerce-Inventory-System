using FluentValidation;

namespace EcommerceInventory.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandValidator
    : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200);

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required.");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0);
    }
}

using EIVMS.Application.Modules.ProductCatalog.DTOs.Product;
using FluentValidation;

namespace EIVMS.Application.Modules.ProductCatalog.Validators;

public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MinimumLength(2).WithMessage("Product name must be at least 2 characters")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required");

        RuleFor(x => x.BasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative");

        RuleFor(x => x.CompareAtPrice)
            .GreaterThan(x => x.BasePrice)
            .When(x => x.CompareAtPrice.HasValue)
            .WithMessage("Compare price must be greater than base price");

        RuleFor(x => x.TaxRate)
            .InclusiveBetween(0, 100)
            .WithMessage("Tax rate must be between 0 and 100");

        RuleFor(x => x.WeightKg)
            .GreaterThan(0)
            .When(x => x.WeightKg.HasValue)
            .WithMessage("Weight must be positive");

        RuleFor(x => x.ShortDescription)
            .MaximumLength(500)
            .WithMessage("Short description cannot exceed 500 characters");

        RuleFor(x => x.MetaTitle)
            .MaximumLength(70)
            .WithMessage("Meta title cannot exceed 70 characters");

        RuleFor(x => x.MetaDescription)
            .MaximumLength(160)
            .WithMessage("Meta description cannot exceed 160 characters");

        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.Count <= 20)
            .WithMessage("Maximum 20 tags allowed");

        When(x => x.DefaultVariant != null, () =>
        {
            RuleFor(x => x.DefaultVariant!.SKU)
                .NotEmpty().WithMessage("Variant SKU is required")
                .MaximumLength(100).WithMessage("SKU cannot exceed 100 characters");
        });
    }
}

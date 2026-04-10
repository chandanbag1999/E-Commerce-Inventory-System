using EIVMS.Application.Modules.ProductCatalog.DTOs.Category;
using FluentValidation;

namespace EIVMS.Application.Modules.ProductCatalog.Validators;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters");

        RuleFor(x => x.CommissionRate)
            .InclusiveBetween(0, 100)
            .When(x => x.CommissionRate.HasValue)
            .WithMessage("Commission rate must be between 0 and 100");

        RuleFor(x => x.MetaTitle)
            .MaximumLength(70)
            .WithMessage("Meta title cannot exceed 70 characters");

        RuleFor(x => x.MetaDescription)
            .MaximumLength(160)
            .WithMessage("Meta description cannot exceed 160 characters");
    }
}

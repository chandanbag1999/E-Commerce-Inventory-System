using FluentValidation;

namespace EcommerceInventory.Application.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandValidator
    : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(150);
    }
}

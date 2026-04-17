using FluentValidation;

namespace EcommerceInventory.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandValidator
    : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(150).WithMessage("Max 150 characters.");
    }
}

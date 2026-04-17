using FluentValidation;

namespace EcommerceInventory.Application.Features.Stocks.Commands.AdjustStock;

public class AdjustStockCommandValidator
    : AbstractValidator<AdjustStockCommand>
{
    private static readonly string[] AllowedTypes = { "Add", "Remove" };

    public AdjustStockCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.WarehouseId)
            .NotEmpty().WithMessage("Warehouse ID is required.");

        RuleFor(x => x.AdjustmentType)
            .NotEmpty().WithMessage("Adjustment type is required.")
            .Must(t => AllowedTypes.Contains(t))
            .WithMessage("Adjustment type must be 'Add' or 'Remove'.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.");
    }
}

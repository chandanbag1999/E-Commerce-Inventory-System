using FluentValidation;

namespace EcommerceInventory.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;

public class CreatePurchaseOrderCommandValidator
    : AbstractValidator<CreatePurchaseOrderCommand>
{
    public CreatePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage("Supplier is required.");

        RuleFor(x => x.WarehouseId)
            .NotEmpty().WithMessage("Warehouse is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item is required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty().WithMessage("Product is required.");
            item.RuleFor(i => i.QuantityOrdered)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
            item.RuleFor(i => i.UnitCost)
                .GreaterThanOrEqualTo(0).WithMessage("Unit cost cannot be negative.");
        });
    }
}

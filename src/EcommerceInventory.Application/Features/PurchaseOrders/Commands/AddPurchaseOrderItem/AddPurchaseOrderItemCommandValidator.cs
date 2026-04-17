using FluentValidation;

namespace EcommerceInventory.Application.Features.PurchaseOrders.Commands.AddPurchaseOrderItem;

public class AddPurchaseOrderItemCommandValidator
    : AbstractValidator<AddPurchaseOrderItemCommand>
{
    public AddPurchaseOrderItemCommandValidator()
    {
        RuleFor(x => x.PurchaseOrderId)
            .NotEmpty().WithMessage("Purchase order ID is required.");
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product is required.");
        RuleFor(x => x.QuantityOrdered)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        RuleFor(x => x.UnitCost)
            .GreaterThanOrEqualTo(0).WithMessage("Unit cost cannot be negative.");
    }
}

using FluentValidation;

namespace EcommerceInventory.Application.Features.PurchaseOrders.Commands.ReceivePurchaseOrder;

public class ReceivePurchaseOrderCommandValidator
    : AbstractValidator<ReceivePurchaseOrderCommand>
{
    public ReceivePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Purchase order ID is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item must be received.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ItemId)
                .NotEmpty().WithMessage("Item ID is required.");
            item.RuleFor(i => i.QuantityReceived)
                .GreaterThan(0).WithMessage("Quantity received must be greater than zero.");
        });
    }
}

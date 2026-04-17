using FluentValidation;

namespace EcommerceInventory.Application.Features.SalesOrders.Commands.AddSalesOrderItem;

public class AddSalesOrderItemCommandValidator
    : AbstractValidator<AddSalesOrderItemCommand>
{
    public AddSalesOrderItemCommandValidator()
    {
        RuleFor(x => x.SalesOrderId)
            .NotEmpty().WithMessage("Sales order ID is required.");
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product is required.");
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative.");
        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount cannot be negative.");
    }
}

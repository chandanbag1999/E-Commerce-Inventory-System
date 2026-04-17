using FluentValidation;

namespace EcommerceInventory.Application.Features.SalesOrders.Commands.CreateSalesOrder;

public class CreateSalesOrderCommandValidator
    : AbstractValidator<CreateSalesOrderCommand>
{
    public CreateSalesOrderCommandValidator()
    {
        RuleFor(x => x.WarehouseId)
            .NotEmpty().WithMessage("Warehouse is required.");

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(200);

        RuleFor(x => x.CustomerEmail)
            .EmailAddress().WithMessage("Invalid email format.")
            .When(x => !string.IsNullOrWhiteSpace(x.CustomerEmail));

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item is required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty().WithMessage("Product is required.");
            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
            item.RuleFor(i => i.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative.");
            item.RuleFor(i => i.Discount)
                .GreaterThanOrEqualTo(0).WithMessage("Discount cannot be negative.");
        });
    }
}

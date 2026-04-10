using EIVMS.Application.Modules.Orders.DTOs;
using FluentValidation;

namespace EIVMS.Application.Modules.Orders.Validators;

public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.IdempotencyKey)
            .NotEmpty().WithMessage("Idempotency key is required")
            .MaximumLength(100).WithMessage("Idempotency key too long");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must have at least one item")
            .Must(items => items.Count <= 50)
            .WithMessage("Maximum 50 items per order");

        RuleForEach(x => x.Items).SetValidator(new CreateOrderItemValidator());

        RuleFor(x => x)
            .Must(x => x.AddressId.HasValue || x.NewAddress != null)
            .WithMessage("Shipping address is required");

        When(x => x.NewAddress != null, () =>
        {
            RuleFor(x => x.NewAddress!.AddressLine1).NotEmpty().WithMessage("Address line 1 is required");
            RuleFor(x => x.NewAddress!.City).NotEmpty().WithMessage("City is required");
            RuleFor(x => x.NewAddress!.ContactPhone).NotEmpty().WithMessage("Contact phone is required");
        });

        RuleFor(x => x.ScheduledDeliveryDate)
            .GreaterThan(DateTime.UtcNow.AddDays(1))
            .When(x => x.ScheduledDeliveryDate.HasValue)
            .WithMessage("Scheduled delivery must be at least 1 day in future");
    }
}

public class CreateOrderItemValidator : AbstractValidator<CreateOrderItemDto>
{
    public CreateOrderItemValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product ID is required");
        RuleFor(x => x.SKU).NotEmpty().WithMessage("SKU is required").MaximumLength(100);
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be positive").LessThanOrEqualTo(1000).WithMessage("Max 1000 units per item");
    }
}
using EIVMS.Application.Modules.Payments.DTOs;
using FluentValidation;

namespace EIVMS.Application.Modules.Payments.Validators;

public class CreatePaymentValidator : AbstractValidator<CreatePaymentRequestDto>
{
    private static readonly string[] AllowedCurrencies = { "INR", "USD", "EUR", "GBP" };
    private static readonly string[] AllowedProviders = { "Razorpay", "Stripe" };

    public CreatePaymentValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0")
            .LessThan(1000000).WithMessage("Amount must be less than 1000000");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Must(c => AllowedCurrencies.Contains(c.ToUpperInvariant()))
            .WithMessage($"Currency must be one of: {string.Join(", ", AllowedCurrencies)}");

        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Provider is required")
            .Must(p => AllowedProviders.Contains(p))
            .WithMessage($"Provider must be one of: {string.Join(", ", AllowedProviders)}");
    }
}

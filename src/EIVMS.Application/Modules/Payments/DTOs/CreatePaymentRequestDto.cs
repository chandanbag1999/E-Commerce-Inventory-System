namespace EIVMS.Application.Modules.Payments.DTOs;

public record CreatePaymentRequestDto(
    Guid OrderId,
    decimal Amount,
    string Currency = "INR",
    string Provider = "Razorpay"
);
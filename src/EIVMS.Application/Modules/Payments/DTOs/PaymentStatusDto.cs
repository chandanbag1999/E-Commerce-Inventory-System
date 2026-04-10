namespace EIVMS.Application.Modules.Payments.DTOs;

public record PaymentAttemptDto(
    string Status,
    string Description,
    DateTime OccurredAt
);

public record PaymentStatusDto(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    string Currency,
    string Status,
    string Provider,
    string? ProviderPaymentId,
    string? FailureReason,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    List<PaymentAttemptDto> Attempts
);
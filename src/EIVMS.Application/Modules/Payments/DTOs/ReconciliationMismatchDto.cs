namespace EIVMS.Application.Modules.Payments.DTOs;

public record ReconciliationMismatchDto(
    Guid PaymentId,
    string DbStatus,
    string GatewayStatus,
    decimal Amount,
    string MismatchType,
    DateTime DetectedAt
);
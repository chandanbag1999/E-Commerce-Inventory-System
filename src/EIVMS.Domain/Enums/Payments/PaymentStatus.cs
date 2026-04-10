namespace EIVMS.Domain.Enums.Payments;

public enum PaymentStatus
{
    Created = 0,
    Pending = 1,
    Success = 2,
    Failed = 3,
    RefundInitiated = 4,
    Refunded = 5,
    PartiallyRefunded = 6,
    Expired = 7,
    Disputed = 8
}
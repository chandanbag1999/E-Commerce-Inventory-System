namespace EIVMS.Domain.Enums.Payments;

public enum RefundStatus
{
    Pending = 0,
    Processing = 1,
    Succeeded = 2,
    Failed = 3,
    Cancelled = 4
}
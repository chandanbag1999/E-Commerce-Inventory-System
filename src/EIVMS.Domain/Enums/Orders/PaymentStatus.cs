namespace EIVMS.Domain.Enums.Orders;

public enum PaymentStatus
{
    Pending = 1,
    Processing = 2,
    Paid = 3,
    Failed = 4,
    Refunded = 5,
    PartialRefunded = 6,
    Cancelled = 7
}
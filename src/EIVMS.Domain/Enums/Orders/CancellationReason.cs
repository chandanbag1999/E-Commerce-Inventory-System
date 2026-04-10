namespace EIVMS.Domain.Enums.Orders;

public enum CancellationReason
{
    CustomerRequest = 1,
    PaymentFailed = 2,
    OutOfStock = 3,
    PricingError = 4,
    FraudDetected = 5,
    DuplicateOrder = 6,
    AddressInvalid = 7,
    VendorCancelled = 8,
    SystemError = 9
}
namespace EIVMS.Domain.Enums.Orders;

public enum ReturnReason
{
    DefectiveProduct = 1,
    WrongItemDelivered = 2,
    NotAsDescribed = 3,
    DamagedInTransit = 4,
    ChangedMind = 5,
    SizeMismatch = 6,
    QualityIssue = 7,
    LateDelivery = 8
}
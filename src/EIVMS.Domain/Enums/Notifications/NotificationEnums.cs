namespace EIVMS.Domain.Enums.Notifications;

public enum NotificationType
{
    Order = 1,
    Stock = 2,
    Delivery = 3,
    System = 4,
    Alert = 5,
    Payment = 6
}

public enum NotificationPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

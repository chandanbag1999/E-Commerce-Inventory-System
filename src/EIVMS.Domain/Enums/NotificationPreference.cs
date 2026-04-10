namespace EIVMS.Domain.Enums;

[Flags]
public enum NotificationPreference
{
    None = 0,
    Email = 1,
    SMS = 2,
    Push = 4,
    WhatsApp = 8,
    All = Email | SMS | Push | WhatsApp
}
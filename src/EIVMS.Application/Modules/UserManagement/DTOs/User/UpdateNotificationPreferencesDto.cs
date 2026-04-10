using EIVMS.Domain.Enums;

namespace EIVMS.Application.Modules.UserManagement.DTOs.User;

public class UpdateNotificationPreferencesDto
{
    public bool EmailNotifications { get; set; }
    public bool SmsNotifications { get; set; }
    public bool PushNotifications { get; set; }
    public bool WhatsAppNotifications { get; set; }

    public NotificationPreference ToNotificationPreference()
    {
        var preference = NotificationPreference.None;

        if (EmailNotifications) preference |= NotificationPreference.Email;
        if (SmsNotifications) preference |= NotificationPreference.SMS;
        if (PushNotifications) preference |= NotificationPreference.Push;
        if (WhatsAppNotifications) preference |= NotificationPreference.WhatsApp;

        return preference;
    }
}
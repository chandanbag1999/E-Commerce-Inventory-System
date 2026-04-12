using EIVMS.Domain.Common;
using EIVMS.Domain.Enums.Notifications;

namespace EIVMS.Domain.Entities.Notifications;

public class Notification : BaseEntity
{
    public Guid UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public string? ActionUrl { get; private set; }
    public NotificationPriority Priority { get; private set; } = NotificationPriority.Medium;

    private Notification() { }

    public static Notification Create(Guid userId, NotificationType type, string title, string message, NotificationPriority priority = NotificationPriority.Medium, string? actionUrl = null)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User id is required", nameof(userId));
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required", nameof(title));
        if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message is required", nameof(message));

        return new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Title = title.Trim(),
            Message = message.Trim(),
            Priority = priority,
            ActionUrl = string.IsNullOrWhiteSpace(actionUrl) ? null : actionUrl.Trim(),
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsRead()
    {
        if (IsRead) return;
        IsRead = true;
        ReadAt = DateTime.UtcNow;
        SetUpdatedAt();
    }
}

using EIVMS.Domain.Entities.Notifications;

namespace EIVMS.Application.Modules.Notifications.Interfaces;

public interface INotificationRepository
{
    Task<List<Notification>> GetByUserIdAsync(Guid userId, int limit = 20);
    Task<Notification?> GetByIdAsync(Guid notificationId, Guid userId);
    Task AddAsync(Notification notification);
    Task AddRangeAsync(IEnumerable<Notification> notifications);
    Task UpdateAsync(Notification notification);
    Task<int> MarkAllAsReadAsync(Guid userId);
    Task DeleteAsync(Notification notification);
    Task<int> ClearAllAsync(Guid userId);
}

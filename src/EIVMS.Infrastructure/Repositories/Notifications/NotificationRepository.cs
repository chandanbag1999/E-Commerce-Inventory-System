using EIVMS.Application.Modules.Notifications.Interfaces;
using EIVMS.Domain.Entities.Notifications;
using EIVMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EIVMS.Infrastructure.Repositories.Notifications;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Notification>> GetByUserIdAsync(Guid userId, int limit = 20)
    {
        return await _context.Set<Notification>()
            .AsNoTracking()
            .Where(notification => notification.UserId == userId)
            .OrderByDescending(notification => notification.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<Notification?> GetByIdAsync(Guid notificationId, Guid userId)
    {
        return await _context.Set<Notification>()
            .FirstOrDefaultAsync(notification => notification.Id == notificationId && notification.UserId == userId);
    }

    public async Task AddAsync(Notification notification)
    {
        await _context.Set<Notification>().AddAsync(notification);
        await _context.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<Notification> notifications)
    {
        await _context.Set<Notification>().AddRangeAsync(notifications);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Notification notification)
    {
        _context.Set<Notification>().Update(notification);
        await _context.SaveChangesAsync();
    }

    public async Task<int> MarkAllAsReadAsync(Guid userId)
    {
        return await _context.Set<Notification>()
            .Where(notification => notification.UserId == userId && !notification.IsRead)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(notification => notification.IsRead, true)
                .SetProperty(notification => notification.ReadAt, DateTime.UtcNow)
                .SetProperty(notification => notification.UpdatedAt, DateTime.UtcNow));
    }

    public async Task DeleteAsync(Notification notification)
    {
        _context.Set<Notification>().Remove(notification);
        await _context.SaveChangesAsync();
    }

    public async Task<int> ClearAllAsync(Guid userId)
    {
        return await _context.Set<Notification>()
            .Where(notification => notification.UserId == userId)
            .ExecuteDeleteAsync();
    }
}

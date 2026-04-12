using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.Notifications.DTOs;
using EIVMS.Domain.Enums.Notifications;

namespace EIVMS.Application.Modules.Notifications.Interfaces;

public interface INotificationService
{
    Task<ApiResponse<List<NotificationDto>>> GetNotificationsAsync(Guid userId, int limit = 20);
    Task<ApiResponse<bool>> MarkAsReadAsync(Guid userId, Guid notificationId);
    Task<ApiResponse<bool>> MarkAllAsReadAsync(Guid userId);
    Task<ApiResponse<bool>> DeleteAsync(Guid userId, Guid notificationId);
    Task<ApiResponse<bool>> ClearAllAsync(Guid userId);
    Task CreateForUserAsync(Guid userId, NotificationType type, string title, string message, NotificationPriority priority = NotificationPriority.Medium, string? actionUrl = null);
    Task CreateForRolesAsync(IEnumerable<string> roleNames, NotificationType type, string title, string message, NotificationPriority priority = NotificationPriority.Medium, string? actionUrl = null);
}

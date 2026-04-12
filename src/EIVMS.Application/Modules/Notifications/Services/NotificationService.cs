using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.Identity.Interfaces;
using EIVMS.Application.Modules.Notifications.DTOs;
using EIVMS.Application.Modules.Notifications.Interfaces;
using EIVMS.Domain.Entities.Notifications;
using EIVMS.Domain.Enums.Notifications;

namespace EIVMS.Application.Modules.Notifications.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserRepository _userRepository;

    public NotificationService(INotificationRepository notificationRepository, IUserRepository userRepository)
    {
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
    }

    public async Task<ApiResponse<List<NotificationDto>>> GetNotificationsAsync(Guid userId, int limit = 20)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(userId, Math.Clamp(limit, 1, 100));
        return ApiResponse<List<NotificationDto>>.SuccessResponse(notifications.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<bool>> MarkAsReadAsync(Guid userId, Guid notificationId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId, userId);
        if (notification == null)
        {
            return ApiResponse<bool>.ErrorResponse("Notification not found", 404);
        }

        notification.MarkAsRead();
        await _notificationRepository.UpdateAsync(notification);
        return ApiResponse<bool>.SuccessResponse(true);
    }

    public async Task<ApiResponse<bool>> MarkAllAsReadAsync(Guid userId)
    {
        await _notificationRepository.MarkAllAsReadAsync(userId);
        return ApiResponse<bool>.SuccessResponse(true);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid userId, Guid notificationId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId, userId);
        if (notification == null)
        {
            return ApiResponse<bool>.ErrorResponse("Notification not found", 404);
        }

        await _notificationRepository.DeleteAsync(notification);
        return ApiResponse<bool>.SuccessResponse(true);
    }

    public async Task<ApiResponse<bool>> ClearAllAsync(Guid userId)
    {
        await _notificationRepository.ClearAllAsync(userId);
        return ApiResponse<bool>.SuccessResponse(true);
    }

    public async Task CreateForUserAsync(Guid userId, NotificationType type, string title, string message, NotificationPriority priority = NotificationPriority.Medium, string? actionUrl = null)
    {
        if (userId == Guid.Empty) return;

        var notification = Notification.Create(userId, type, title, message, priority, actionUrl);
        await _notificationRepository.AddAsync(notification);
    }

    public async Task CreateForRolesAsync(IEnumerable<string> roleNames, NotificationType type, string title, string message, NotificationPriority priority = NotificationPriority.Medium, string? actionUrl = null)
    {
        var normalizedRoleNames = roleNames
            .Where(roleName => !string.IsNullOrWhiteSpace(roleName))
            .Select(roleName => roleName.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (normalizedRoleNames.Length == 0) return;

        var users = await _userRepository.GetUsersByRoleNamesAsync(normalizedRoleNames);
        var notifications = users
            .Select(user => Notification.Create(user.Id, type, title, message, priority, actionUrl))
            .ToList();

        if (notifications.Count == 0) return;

        await _notificationRepository.AddRangeAsync(notifications);
    }

    private static NotificationDto MapToDto(Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            Type = notification.Type.ToString().ToLowerInvariant(),
            Title = notification.Title,
            Message = notification.Message,
            Read = notification.IsRead,
            ActionUrl = notification.ActionUrl,
            CreatedAt = notification.CreatedAt,
            Priority = notification.Priority.ToString().ToLowerInvariant(),
            UserId = notification.UserId,
        };
    }
}

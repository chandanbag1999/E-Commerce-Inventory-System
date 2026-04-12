using EIVMS.Application.Modules.Identity.Interfaces;
using EIVMS.Application.Modules.Notifications.Interfaces;
using EIVMS.Application.Modules.Notifications.Services;
using EIVMS.Domain.Entities.Identity;
using EIVMS.Domain.Entities.Notifications;
using EIVMS.Domain.Enums.Notifications;
using FluentAssertions;
using Moq;

namespace EIVMS.UnitTests.Modules.Notifications;

public class NotificationServiceTests
{
    [Fact]
    public async Task CreateForRolesAsync_ShouldCreateOneNotificationPerResolvedUser()
    {
        var notificationRepositoryMock = new Mock<INotificationRepository>();
        var userRepositoryMock = new Mock<IUserRepository>();
        var service = new NotificationService(notificationRepositoryMock.Object, userRepositoryMock.Object);

        var users = new List<User>
        {
            User.Create("Asha", "Admin", "asha@example.com", "hash-1"),
            User.Create("Ravi", "Warehouse", "ravi@example.com", "hash-2")
        };

        List<Notification>? createdNotifications = null;

        userRepositoryMock
            .Setup(repository => repository.GetUsersByRoleNamesAsync(It.IsAny<string[]>()))
            .ReturnsAsync(users);

        notificationRepositoryMock
            .Setup(repository => repository.AddRangeAsync(It.IsAny<IEnumerable<Notification>>()))
            .Callback<IEnumerable<Notification>>(notifications => createdNotifications = notifications.ToList())
            .Returns(Task.CompletedTask);

        await service.CreateForRolesAsync(
            ["Admin", "InventoryManager"],
            NotificationType.Stock,
            "Low Stock Alert",
            "Inventory requires attention.",
            NotificationPriority.High,
            "/inventory");

        createdNotifications.Should().NotBeNull();
        createdNotifications!.Should().HaveCount(2);
        createdNotifications!.Select(notification => notification.UserId).Should().BeEquivalentTo(users.Select(user => user.Id));
        createdNotifications!.All(notification => notification.Priority == NotificationPriority.High).Should().BeTrue();
    }

    [Fact]
    public async Task MarkAsReadAsync_WhenNotificationExists_ShouldUpdateNotification()
    {
        var notificationRepositoryMock = new Mock<INotificationRepository>();
        var userRepositoryMock = new Mock<IUserRepository>();
        var service = new NotificationService(notificationRepositoryMock.Object, userRepositoryMock.Object);

        var userId = Guid.NewGuid();
        var notification = Notification.Create(userId, NotificationType.Order, "New Order", "Order created.");

        notificationRepositoryMock
            .Setup(repository => repository.GetByIdAsync(notification.Id, userId))
            .ReturnsAsync(notification);

        notificationRepositoryMock
            .Setup(repository => repository.UpdateAsync(notification))
            .Returns(Task.CompletedTask);

        var result = await service.MarkAsReadAsync(userId, notification.Id);

        result.Success.Should().BeTrue();
        notification.IsRead.Should().BeTrue();
        notificationRepositoryMock.Verify(repository => repository.UpdateAsync(notification), Times.Once);
    }
}

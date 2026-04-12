using EIVMS.Application.Modules.Notifications.Interfaces;
using EIVMS.Application.Modules.Orders.DTOs;
using EIVMS.Application.Modules.Orders.Interfaces;
using EIVMS.Application.Modules.Orders.Services;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace EIVMS.UnitTests.Modules.Orders;

public class OrderServiceTests
{
    [Fact]
    public async Task GetAllOrdersAsync_WithLimit_ShouldNormalizePagingForHomeFeed()
    {
        var orderRepositoryMock = new Mock<IOrderRepository>();
        var inventoryServiceMock = new Mock<IInventoryIntegrationService>();
        var productServiceMock = new Mock<IProductIntegrationService>();
        var notificationServiceMock = new Mock<INotificationService>();
        var mediatorMock = new Mock<IMediator>();
        var validatorMock = new Mock<IValidator<CreateOrderDto>>();
        var loggerMock = new Mock<ILogger<OrderService>>();

        OrderFilterDto? capturedFilter = null;

        orderRepositoryMock
            .Setup(repository => repository.GetPagedAsync(It.IsAny<OrderFilterDto>()))
            .Callback<OrderFilterDto>(filter => capturedFilter = filter)
            .ReturnsAsync((new List<EIVMS.Domain.Entities.Orders.Order>(), 0));

        var service = new OrderService(
            orderRepositoryMock.Object,
            inventoryServiceMock.Object,
            productServiceMock.Object,
            notificationServiceMock.Object,
            mediatorMock.Object,
            validatorMock.Object,
            loggerMock.Object);

        var result = await service.GetAllOrdersAsync(new OrderFilterDto
        {
            Limit = 5,
            PageNumber = 4,
            PageSize = 25
        });

        result.Success.Should().BeTrue();
        capturedFilter.Should().NotBeNull();
        capturedFilter!.PageNumber.Should().Be(1);
        capturedFilter.PageSize.Should().Be(5);
        result.Data!.PageSize.Should().Be(5);
    }
}

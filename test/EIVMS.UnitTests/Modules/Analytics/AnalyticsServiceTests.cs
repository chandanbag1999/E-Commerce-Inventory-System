using EIVMS.Application.Modules.Analytics.DTOs;
using EIVMS.Domain.Entities.Inventory;
using EIVMS.Domain.Entities.Orders;
using EIVMS.Domain.Entities.ProductCatalog;
using EIVMS.Domain.Enums.Inventory;
using EIVMS.Domain.Enums.Orders;
using EIVMS.Infrastructure.Persistence;
using EIVMS.Infrastructure.Services.Analytics;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EIVMS.UnitTests.Modules.Analytics;

public class AnalyticsServiceTests
{
    [Fact]
    public async Task GetDashboardAsync_WithSeededData_ShouldReturnAggregatedDashboardSections()
    {
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        using (var seedContext = new AppDbContext(options))
        {
            var category = Category.Create("Electronics", "electronics");
            var product = Product.Create("Phone", "phone", category.Id, 1200m, Guid.NewGuid());
            product.Activate();

            var warehouse = Warehouse.Create("Main Warehouse", "MAIN", "Street 1", "Delhi", "Delhi", "India", "110001", 28.61, 77.20);
            warehouse.SetCapacity(100);

            var inventoryItem = InventoryItem.Create(product.Id, "SKU-001", warehouse.Id, 12, 20);
            var stockMovement = StockMovement.Create(inventoryItem.Id, product.Id, inventoryItem.SKU, warehouse.Id, StockMovementType.StockIn, 12, 0, 12);
            var alert = InventoryAlert.Create(inventoryItem.Id, product.Id, inventoryItem.SKU, warehouse.Id, InventoryAlertType.LowStock, inventoryItem.AvailableQuantity, inventoryItem.LowStockThreshold);

            var deliveredOrder = BuildPaidOrder(product, "idem-delivered", PaymentMethod.UPI, "Riya Shah", 1200m, 50m, 216m);
            deliveredOrder.StartProcessing();
            deliveredOrder.MarkAsPacked();
            deliveredOrder.MarkAsShipped("TRK-1", "BlueDart");
            deliveredOrder.MarkOutForDelivery();
            deliveredOrder.MarkAsDelivered();

            var pipelineOrder = BuildPaidOrder(product, "idem-pipeline", PaymentMethod.UPI, "Aman Verma", 1800m, 60m, 324m);
            pipelineOrder.StartProcessing();
            pipelineOrder.MarkAsPacked();
            pipelineOrder.MarkAsShipped("TRK-2", "Delhivery");

            await seedContext.Categories.AddAsync(category);
            await seedContext.Products.AddAsync(product);
            await seedContext.Warehouses.AddAsync(warehouse);
            await seedContext.InventoryItems.AddAsync(inventoryItem);
            await seedContext.StockMovements.AddAsync(stockMovement);
            await seedContext.InventoryAlerts.AddAsync(alert);
            await seedContext.Orders.AddRangeAsync(deliveredOrder, pipelineOrder);
            await seedContext.SaveChangesAsync();
        }

        using var testContext = new AppDbContext(options);
        var service = new AnalyticsService(testContext);

        var result = await service.GetDashboardAsync(Guid.NewGuid());

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Stats.Admin.Should().HaveCount(4);
        result.Data.Stats.Warehouse.Should().HaveCount(4);
        result.Data.Stats.Delivery.Should().HaveCount(4);
        result.Data.RevenueOverview.Should().HaveCount(6);
        result.Data.OrdersThisWeek.Should().HaveCount(7);
        result.Data.SalesByCategory.Should().Contain(entry => entry.Name == "Electronics" && entry.Value > 0);
        result.Data.RecentActivity.Should().NotBeEmpty();
    }

    private static Order BuildPaidOrder(Product product, string idempotencyKey, PaymentMethod paymentMethod, string contactName, decimal subtotal, decimal shipping, decimal tax)
    {
        var order = Order.Create(Guid.NewGuid(), idempotencyKey, paymentMethod);
        order.SetShippingAddress("Street 1", string.Empty, "Delhi", "Delhi", "India", "110001", contactName, "+91-9999999999");

        var orderItem = OrderItem.Create(order.Id, product.Id, "SKU-001", product.Name, subtotal, subtotal, 1, 18m);
        order.AddItem(orderItem);
        order.SetPricing(subtotal, shipping, tax);
        order.Confirm("txn-123");
        return order;
    }
}

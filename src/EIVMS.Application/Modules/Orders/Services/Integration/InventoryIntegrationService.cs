using EIVMS.Application.Modules.Inventory.Interfaces;
using EIVMS.Application.Modules.Orders.Interfaces;
using EIVMS.Domain.Entities.Inventory;
using EIVMS.Domain.Enums.Inventory;
using Microsoft.Extensions.Logging;

namespace EIVMS.Application.Modules.Orders.Services.Integration;

public class InventoryIntegrationService : IInventoryIntegrationService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ILogger<InventoryIntegrationService> _logger;

    public InventoryIntegrationService(IInventoryRepository inventoryRepository, ILogger<InventoryIntegrationService> logger)
    {
        _inventoryRepository = inventoryRepository;
        _logger = logger;
    }

    public async Task<bool> IsStockAvailableAsync(string sku, int quantity)
    {
        var warehouse = await _inventoryRepository.GetDefaultWarehouseAsync();
        if (warehouse == null)
        {
            _logger.LogWarning("No default warehouse found");
            return false;
        }

        var item = await _inventoryRepository.GetInventoryItemBySkuAndWarehouseAsync(sku, warehouse.Id);
        if (item == null)
        {
            _logger.LogWarning("Inventory item not found for SKU: {SKU}", sku);
            return false;
        }

        return item.AvailableQuantity >= quantity;
    }

    public async Task<bool> ReserveStockAsync(string sku, int quantity, string orderId, Guid userId, double? latitude = null, double? longitude = null)
    {
        var warehouse = await GetBestWarehouse(latitude, longitude);
        if (warehouse == null)
        {
            _logger.LogWarning("No suitable warehouse found");
            return false;
        }

        var item = await _inventoryRepository.GetInventoryItemBySkuAndWarehouseAsync(sku, warehouse.Id);
        if (item == null)
        {
            _logger.LogWarning("Inventory item not found for SKU: {SKU}", sku);
            return false;
        }

        if (item.AvailableQuantity < quantity)
        {
            _logger.LogWarning("Insufficient stock for SKU: {SKU}. Available: {Available}, Requested: {Requested}", sku, item.AvailableQuantity, quantity);
            return false;
        }

        var reservation = StockReservation.Create(
            item.Id,
            item.ProductId,
            item.SKU,
            warehouse.Id,
            orderId,
            userId,
            quantity,
            30
        );

        await _inventoryRepository.AddStockReservationAsync(reservation);

        item.Reserve(quantity);
        await _inventoryRepository.UpdateInventoryItemAsync(item);

        _logger.LogInformation("Stock reserved for SKU: {SKU}, Quantity: {Quantity}, OrderId: {OrderId}", sku, quantity, orderId);
        return true;
    }

    public async Task<(bool Success, List<string> Failures)> BulkReserveAsync(
        List<(string SKU, int Quantity, Guid ProductId)> items,
        string orderId,
        Guid userId,
        double? latitude = null,
        double? longitude = null)
    {
        var failures = new List<string>();
        var reservations = new List<(StockReservation Reservation, InventoryItem Item)>();

        var warehouse = await GetBestWarehouse(latitude, longitude);
        if (warehouse == null)
        {
            return (false, new List<string> { "No suitable warehouse found" });
        }

        foreach (var (sku, quantity, productId) in items)
        {
            var item = await _inventoryRepository.GetInventoryItemBySkuAndWarehouseAsync(sku, warehouse.Id);
            if (item == null)
            {
                failures.Add($"{sku}: Product not found in inventory");
                continue;
            }

            if (item.AvailableQuantity < quantity)
            {
                failures.Add($"{sku}: Insufficient stock (Available: {item.AvailableQuantity}, Requested: {quantity})");
                continue;
            }

            var reservation = StockReservation.Create(
                item.Id,
                item.ProductId,
                item.SKU,
                warehouse.Id,
                orderId,
                userId,
                quantity,
                30
            );

            reservations.Add((reservation, item));
        }

        if (failures.Any())
        {
            _logger.LogWarning("Bulk reserve failed for order {OrderId}. Failures: {Failures}", orderId, string.Join(", ", failures));
            return (false, failures);
        }

        foreach (var (reservation, item) in reservations)
        {
            await _inventoryRepository.AddStockReservationAsync(reservation);
            item.Reserve(reservation.Quantity);
            await _inventoryRepository.UpdateInventoryItemAsync(item);
        }

        _logger.LogInformation("Bulk reserve successful for order {OrderId}, Items: {ItemCount}", orderId, reservations.Count);
        return (true, new List<string>());
    }

    public async Task ReleaseStockAsync(string orderId)
    {
        var reservations = await _inventoryRepository.GetReservationsByOrderIdAsync(orderId);
        if (!reservations.Any())
        {
            _logger.LogWarning("No stock reservation found for order: {OrderId}", orderId);
            return;
        }

        foreach (var reservation in reservations)
        {
            var item = await _inventoryRepository.GetInventoryItemByIdAsync(reservation.InventoryItemId);
            if (item != null)
            {
                item.Release(reservation.Quantity);
                await _inventoryRepository.UpdateInventoryItemAsync(item);
            }

            reservation.Release("Order cancelled");
            await _inventoryRepository.UpdateStockReservationAsync(reservation);
        }

        _logger.LogInformation("Stock released for order: {OrderId}", orderId);
    }

    public async Task ConfirmStockAsync(string orderId)
    {
        var reservations = await _inventoryRepository.GetReservationsByOrderIdAsync(orderId);
        if (!reservations.Any())
        {
            _logger.LogWarning("No stock reservation found for order: {OrderId}", orderId);
            return;
        }

        foreach (var reservation in reservations)
        {
            var item = await _inventoryRepository.GetInventoryItemByIdAsync(reservation.InventoryItemId);
            if (item != null)
            {
                var stockBefore = item.TotalQuantity;
                item.ConfirmReservation(reservation.Quantity);
                var stockAfter = item.TotalQuantity;
                await _inventoryRepository.UpdateInventoryItemAsync(item);

                var movement = StockMovement.Create(
                    item.Id,
                    item.ProductId,
                    item.SKU,
                    reservation.WarehouseId,
                    StockMovementType.StockOut,
                    -reservation.Quantity,
                    stockBefore,
                    stockAfter,
                    orderId,
                    "Order",
                    "Order confirmed"
                );
                await _inventoryRepository.AddStockMovementAsync(movement);
            }

            reservation.Confirm();
            await _inventoryRepository.UpdateStockReservationAsync(reservation);
        }

        _logger.LogInformation("Stock confirmed for order: {OrderId}", orderId);
    }

    private async Task<Warehouse?> GetBestWarehouse(double? latitude, double? longitude)
    {
        if (latitude.HasValue && longitude.HasValue)
        {
            var nearbyWarehouses = await _inventoryRepository.GetWarehousesNearLocationAsync(latitude.Value, longitude.Value, 50);
            return nearbyWarehouses.FirstOrDefault();
        }

        return await _inventoryRepository.GetDefaultWarehouseAsync();
    }
}
using EIVMS.Application.Modules.Inventory.DTOs;
using EIVMS.Domain.Entities.Inventory;
using EIVMS.Domain.Enums.Inventory;

namespace EIVMS.Application.Modules.Inventory.Interfaces;

public interface IInventoryRepository
{
    Task<Warehouse?> GetWarehouseByIdAsync(Guid id);
    Task<Warehouse?> GetWarehouseByCodeAsync(string code);
    Task<List<Warehouse>> GetAllWarehousesAsync(bool includeInactive = false);
    Task<List<Warehouse>> GetWarehousesNearLocationAsync(double latitude, double longitude, double radiusKm);
    Task<Warehouse?> GetDefaultWarehouseAsync();
    Task AddWarehouseAsync(Warehouse warehouse);
    Task UpdateWarehouseAsync(Warehouse warehouse);

    Task<InventoryItem?> GetInventoryItemByIdAsync(Guid id);
    Task<InventoryItem?> GetInventoryItemBySkuAndWarehouseAsync(string sku, Guid warehouseId);
    Task<List<InventoryItem>> GetInventoryItemsByWarehouseAsync(Guid warehouseId);
    Task<List<InventoryItem>> GetInventoryItemsByProductAsync(Guid productId);
    Task<List<InventoryItem>> GetLowStockItemsAsync(Guid? warehouseId = null);
    Task<List<InventoryItem>> GetOutOfStockItemsAsync(Guid? warehouseId = null);
    Task<List<InventoryItem>> GetNearExpiryItemsAsync(int daysAhead, Guid? warehouseId = null);
    Task AddInventoryItemAsync(InventoryItem item);
    Task UpdateInventoryItemAsync(InventoryItem item);
    Task<bool> UpdateInventoryItemWithOptimisticLockAsync(InventoryItem item, int expectedVersion);

    Task<StockMovement?> GetStockMovementByIdAsync(Guid id);
    Task<List<StockMovement>> GetStockMovementsByInventoryItemAsync(Guid inventoryItemId);
    Task<List<StockMovement>> GetStockMovementsByWarehouseAsync(Guid warehouseId, DateTime? fromDate = null, DateTime? toDate = null);
    Task AddStockMovementAsync(StockMovement movement);

    Task<StockReservation?> GetStockReservationByIdAsync(Guid id);
    Task<StockReservation?> GetStockReservationByCodeAsync(string reservationCode);
    Task<List<StockReservation>> GetActiveReservationsByInventoryItemAsync(Guid inventoryItemId);
    Task<List<StockReservation>> GetExpiredReservationsAsync();
    Task<List<StockReservation>> GetReservationsByOrderIdAsync(string orderId);
    Task AddStockReservationAsync(StockReservation reservation);
    Task UpdateStockReservationAsync(StockReservation reservation);

    Task<StockTransfer?> GetStockTransferByIdAsync(Guid id);
    Task<List<StockTransfer>> GetPendingTransfersAsync();
    Task<List<StockTransfer>> GetTransfersByWarehouseAsync(Guid warehouseId);
    Task AddStockTransferAsync(StockTransfer transfer);
    Task UpdateStockTransferAsync(StockTransfer transfer);

    Task<InventoryAlert?> GetInventoryAlertByIdAsync(Guid id);
    Task<List<InventoryAlert>> GetUnresolvedAlertsAsync(Guid? warehouseId = null, InventoryAlertType? type = null);
    Task<List<InventoryAlert>> GetAlertsByInventoryItemAsync(Guid inventoryItemId);
    Task AddInventoryAlertAsync(InventoryAlert alert);
    Task UpdateInventoryAlertAsync(InventoryAlert alert);
}
using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.Inventory.DTOs;
using EIVMS.Domain.Enums.Inventory;

namespace EIVMS.Application.Modules.Inventory.Interfaces;

public interface IInventoryService
{
    Task<ApiResponse<WarehouseDto>> GetWarehouseByIdAsync(Guid id);
    Task<ApiResponse<List<WarehouseDto>>> GetAllWarehousesAsync(bool includeInactive = false);
    Task<ApiResponse<List<WarehouseDto>>> GetWarehousesNearLocationAsync(double latitude, double longitude, double radiusKm);
    Task<ApiResponse<WarehouseDto>> CreateWarehouseAsync(CreateWarehouseDto dto);
    Task<ApiResponse<WarehouseDto>> UpdateWarehouseAsync(Guid id, CreateWarehouseDto dto);
    Task<ApiResponse<bool>> DeleteWarehouseAsync(Guid id);

    Task<ApiResponse<InventoryItemDto>> GetInventoryItemByIdAsync(Guid id);
    Task<ApiResponse<List<InventoryItemDto>>> GetInventoryItemsByWarehouseAsync(Guid warehouseId);
    Task<ApiResponse<List<InventoryItemDto>>> GetInventoryItemsByProductAsync(Guid productId);
    Task<ApiResponse<List<InventoryItemDto>>> GetLowStockItemsAsync(Guid? warehouseId = null);
    Task<ApiResponse<List<InventoryItemDto>>> GetOutOfStockItemsAsync(Guid? warehouseId = null);
    Task<ApiResponse<InventoryItemDto>> CreateInventoryItemAsync(Guid productId, string sku, Guid warehouseId, int initialQuantity, int lowStockThreshold = 10);
    Task<ApiResponse<bool>> AdjustInventoryItemAsync(Guid id, int newQuantity, string reason, Guid userId);

    Task<ApiResponse<StockMovementDto>> AddStockAsync(StockOperationDto dto, Guid userId);
    Task<ApiResponse<StockMovementDto>> RemoveStockAsync(StockOperationDto dto, Guid userId);
    Task<ApiResponse<StockMovementDto>> RecordDamagedStockAsync(StockOperationDto dto, Guid userId);
    Task<ApiResponse<List<StockMovementDto>>> GetStockMovementsAsync(Guid inventoryItemId);

    Task<ApiResponse<StockReservationDto>> ReserveStockAsync(ReserveStockDto dto);
    Task<ApiResponse<StockReservationDto>> GetReservationByCodeAsync(string reservationCode);
    Task<ApiResponse<bool>> ConfirmReservationAsync(Guid reservationId, Guid userId);
    Task<ApiResponse<bool>> ReleaseReservationAsync(Guid reservationId);
    Task<ApiResponse<bool>> ProcessExpiredReservationsAsync();

    Task<ApiResponse<StockTransferDto>> CreateStockTransferAsync(CreateStockTransferDto dto, Guid userId);
    Task<ApiResponse<StockTransferDto>> GetTransferByIdAsync(Guid id);
    Task<ApiResponse<StockTransferDto>> ShipTransferAsync(Guid transferId, Guid userId);
    Task<ApiResponse<StockTransferDto>> ReceiveTransferAsync(Guid transferId, Guid userId);
    Task<ApiResponse<StockTransferDto>> CancelTransferAsync(Guid transferId, Guid userId);
    Task<ApiResponse<List<StockTransferDto>>> GetPendingTransfersAsync();

    Task<ApiResponse<List<InventoryAlertDto>>> GetAlertsAsync(Guid? warehouseId = null, InventoryAlertType? type = null);
    Task<ApiResponse<bool>> MarkAlertAsReadAsync(Guid alertId);
    Task<ApiResponse<bool>> ResolveAlertAsync(Guid alertId);
    Task<ApiResponse<bool>> GenerateStockAlertsAsync();
}
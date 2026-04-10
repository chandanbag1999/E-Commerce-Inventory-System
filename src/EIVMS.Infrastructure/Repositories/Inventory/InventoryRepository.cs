using EIVMS.Application.Modules.Inventory.Interfaces;
using EIVMS.Domain.Entities.Inventory;
using EIVMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EIVMS.Infrastructure.Repositories.Inventory;

public class InventoryRepository : IInventoryRepository
{
    private readonly AppDbContext _context;

    public InventoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Warehouse?> GetWarehouseByIdAsync(Guid id)
    {
        return await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);
    }

    public async Task<Warehouse?> GetWarehouseByCodeAsync(string code)
    {
        return await _context.Warehouses.FirstOrDefaultAsync(w => w.Code == code.ToUpperInvariant() && !w.IsDeleted);
    }

    public async Task<List<Warehouse>> GetAllWarehousesAsync(bool includeInactive = false)
    {
        var query = _context.Warehouses.Where(w => !w.IsDeleted);
        if (!includeInactive)
            query = query.Where(w => w.Status == Domain.Enums.Inventory.WarehouseStatus.Active);
        return await query.OrderBy(w => w.Priority).ThenBy(w => w.Name).ToListAsync();
    }

    public async Task<List<Warehouse>> GetWarehousesNearLocationAsync(double latitude, double longitude, double radiusKm)
    {
        var warehouses = await _context.Warehouses
            .Where(w => w.Status == Domain.Enums.Inventory.WarehouseStatus.Active && !w.IsDeleted)
            .ToListAsync();

        return warehouses
            .Where(w => w.DistanceTo(latitude, longitude) <= radiusKm)
            .OrderBy(w => w.DistanceTo(latitude, longitude))
            .ToList();
    }

    public async Task<Warehouse?> GetDefaultWarehouseAsync()
    {
        return await _context.Warehouses.FirstOrDefaultAsync(w => w.IsDefault && !w.IsDeleted);
    }

    public async Task AddWarehouseAsync(Warehouse warehouse)
    {
        await _context.Warehouses.AddAsync(warehouse);
    }

    public async Task UpdateWarehouseAsync(Warehouse warehouse)
    {
        _context.Warehouses.Update(warehouse);
    }

    public async Task<InventoryItem?> GetInventoryItemByIdAsync(Guid id)
    {
        return await _context.InventoryItems
            .Include(i => i.Warehouse)
            .FirstOrDefaultAsync(i => i.Id == id && i.IsActive);
    }

    public async Task<InventoryItem?> GetInventoryItemBySkuAndWarehouseAsync(string sku, Guid warehouseId)
    {
        return await _context.InventoryItems
            .Include(i => i.Warehouse)
            .FirstOrDefaultAsync(i => i.SKU == sku.ToUpperInvariant() && i.WarehouseId == warehouseId && i.IsActive);
    }

    public async Task<List<InventoryItem>> GetInventoryItemsByWarehouseAsync(Guid warehouseId)
    {
        return await _context.InventoryItems
            .Include(i => i.Warehouse)
            .Where(i => i.WarehouseId == warehouseId && i.IsActive)
            .ToListAsync();
    }

    public async Task<List<InventoryItem>> GetInventoryItemsByProductAsync(Guid productId)
    {
        return await _context.InventoryItems
            .Include(i => i.Warehouse)
            .Where(i => i.ProductId == productId && i.IsActive)
            .ToListAsync();
    }

    public async Task<List<InventoryItem>> GetLowStockItemsAsync(Guid? warehouseId = null)
    {
        var query = _context.InventoryItems
            .Include(i => i.Warehouse)
            .Where(i => i.IsActive && i.TotalQuantity - i.ReservedQuantity - i.DamagedQuantity <= i.LowStockThreshold && i.TotalQuantity - i.ReservedQuantity - i.DamagedQuantity > 0);

        if (warehouseId.HasValue)
            query = query.Where(i => i.WarehouseId == warehouseId.Value);

        return await query.ToListAsync();
    }

    public async Task<List<InventoryItem>> GetOutOfStockItemsAsync(Guid? warehouseId = null)
    {
        var query = _context.InventoryItems
            .Include(i => i.Warehouse)
            .Where(i => i.IsActive && i.TotalQuantity - i.ReservedQuantity - i.DamagedQuantity <= 0);

        if (warehouseId.HasValue)
            query = query.Where(i => i.WarehouseId == warehouseId.Value);

        return await query.ToListAsync();
    }

    public async Task<List<InventoryItem>> GetNearExpiryItemsAsync(int daysAhead, Guid? warehouseId = null)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);
        var query = _context.InventoryItems
            .Include(i => i.Warehouse)
            .Where(i => i.IsActive && i.ExpiryDate != null && i.ExpiryDate <= cutoffDate && i.ExpiryDate > DateTime.UtcNow);

        if (warehouseId.HasValue)
            query = query.Where(i => i.WarehouseId == warehouseId.Value);

        return await query.ToListAsync();
    }

    public async Task AddInventoryItemAsync(InventoryItem item)
    {
        await _context.InventoryItems.AddAsync(item);
    }

    public async Task UpdateInventoryItemAsync(InventoryItem item)
    {
        _context.InventoryItems.Update(item);
    }

    public async Task<bool> UpdateInventoryItemWithOptimisticLockAsync(InventoryItem item, int expectedVersion)
    {
        var affected = await _context.InventoryItems
            .Where(i => i.Id == item.Id && i.Version == expectedVersion)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(i => i.TotalQuantity, item.TotalQuantity)
                .SetProperty(i => i.ReservedQuantity, item.ReservedQuantity)
                .SetProperty(i => i.DamagedQuantity, item.DamagedQuantity)
                .SetProperty(i => i.AverageCost, item.AverageCost)
                .SetProperty(i => i.Version, item.Version + 1)
                .SetProperty(i => i.UpdatedAt, DateTime.UtcNow));

        return affected > 0;
    }

    public async Task<StockMovement?> GetStockMovementByIdAsync(Guid id)
    {
        return await _context.StockMovements
            .Include(m => m.InventoryItem)
            .Include(m => m.Warehouse)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<List<StockMovement>> GetStockMovementsByInventoryItemAsync(Guid inventoryItemId)
    {
        return await _context.StockMovements
            .Where(m => m.InventoryItemId == inventoryItemId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<StockMovement>> GetStockMovementsByWarehouseAsync(Guid warehouseId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.StockMovements
            .Include(m => m.InventoryItem)
            .Where(m => m.WarehouseId == warehouseId);

        if (fromDate.HasValue)
            query = query.Where(m => m.CreatedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(m => m.CreatedAt <= toDate.Value);

        return await query.OrderByDescending(m => m.CreatedAt).ToListAsync();
    }

    public async Task AddStockMovementAsync(StockMovement movement)
    {
        await _context.StockMovements.AddAsync(movement);
    }

    public async Task<StockReservation?> GetStockReservationByIdAsync(Guid id)
    {
        return await _context.StockReservations
            .Include(r => r.InventoryItem)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<StockReservation?> GetStockReservationByCodeAsync(string reservationCode)
    {
        return await _context.StockReservations
            .Include(r => r.InventoryItem)
            .FirstOrDefaultAsync(r => r.OrderId == reservationCode);
    }

    public async Task<List<StockReservation>> GetActiveReservationsByInventoryItemAsync(Guid inventoryItemId)
    {
        return await _context.StockReservations
            .Where(r => r.InventoryItemId == inventoryItemId && r.Status == Domain.Enums.Inventory.StockReservationStatus.Pending)
            .ToListAsync();
    }

    public async Task<List<StockReservation>> GetExpiredReservationsAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.StockReservations
            .Where(r => r.Status == Domain.Enums.Inventory.StockReservationStatus.Pending && r.ExpiresAt < now)
            .ToListAsync();
    }

    public async Task<List<StockReservation>> GetReservationsByOrderIdAsync(string orderId)
    {
        return await _context.StockReservations
            .Include(r => r.InventoryItem)
            .Where(r => r.OrderId == orderId)
            .ToListAsync();
    }

    public async Task AddStockReservationAsync(StockReservation reservation)
    {
        await _context.StockReservations.AddAsync(reservation);
    }

    public async Task UpdateStockReservationAsync(StockReservation reservation)
    {
        _context.StockReservations.Update(reservation);
    }

    public async Task<StockTransfer?> GetStockTransferByIdAsync(Guid id)
    {
        return await _context.StockTransfers
            .Include(t => t.SourceWarehouse)
            .Include(t => t.DestinationWarehouse)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<StockTransfer>> GetPendingTransfersAsync()
    {
        return await _context.StockTransfers
            .Include(t => t.SourceWarehouse)
            .Include(t => t.DestinationWarehouse)
            .Where(t => t.Status == "Pending" || t.Status == "InTransit")
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<StockTransfer>> GetTransfersByWarehouseAsync(Guid warehouseId)
    {
        return await _context.StockTransfers
            .Include(t => t.SourceWarehouse)
            .Include(t => t.DestinationWarehouse)
            .Where(t => t.SourceWarehouseId == warehouseId || t.DestinationWarehouseId == warehouseId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task AddStockTransferAsync(StockTransfer transfer)
    {
        await _context.StockTransfers.AddAsync(transfer);
    }

    public async Task UpdateStockTransferAsync(StockTransfer transfer)
    {
        _context.StockTransfers.Update(transfer);
    }

    public async Task<InventoryAlert?> GetInventoryAlertByIdAsync(Guid id)
    {
        return await _context.InventoryAlerts
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<InventoryAlert>> GetUnresolvedAlertsAsync(Guid? warehouseId = null, Domain.Enums.Inventory.InventoryAlertType? type = null)
    {
        var query = _context.InventoryAlerts
            .Where(a => a.ResolvedAt == null);

        if (warehouseId.HasValue)
            query = query.Where(a => a.WarehouseId == warehouseId.Value);
        if (type.HasValue)
            query = query.Where(a => a.Type == type.Value);

        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
    }

    public async Task<List<InventoryAlert>> GetAlertsByInventoryItemAsync(Guid inventoryItemId)
    {
        return await _context.InventoryAlerts
            .Where(a => a.InventoryItemId == inventoryItemId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task AddInventoryAlertAsync(InventoryAlert alert)
    {
        await _context.InventoryAlerts.AddAsync(alert);
    }

    public async Task UpdateInventoryAlertAsync(InventoryAlert alert)
    {
        _context.InventoryAlerts.Update(alert);
    }
}
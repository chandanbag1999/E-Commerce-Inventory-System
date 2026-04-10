using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.Inventory.DTOs;
using EIVMS.Application.Modules.Inventory.Interfaces;
using EIVMS.Domain.Entities.Inventory;
using EIVMS.Domain.Enums.Inventory;

namespace EIVMS.Application.Modules.Inventory.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _repository;

    public InventoryService(IInventoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<WarehouseDto>> GetWarehouseByIdAsync(Guid id)
    {
        var warehouse = await _repository.GetWarehouseByIdAsync(id);
        if (warehouse == null || warehouse.IsDeleted)
            return ApiResponse<WarehouseDto>.ErrorResponse("Warehouse not found", 404);
        return ApiResponse<WarehouseDto>.SuccessResponse(MapWarehouseToDto(warehouse));
    }

    public async Task<ApiResponse<List<WarehouseDto>>> GetAllWarehousesAsync(bool includeInactive = false)
    {
        var warehouses = await _repository.GetAllWarehousesAsync(includeInactive);
        return ApiResponse<List<WarehouseDto>>.SuccessResponse(warehouses.Select(MapWarehouseToDto).ToList());
    }

    public async Task<ApiResponse<List<WarehouseDto>>> GetWarehousesNearLocationAsync(double latitude, double longitude, double radiusKm)
    {
        var warehouses = await _repository.GetWarehousesNearLocationAsync(latitude, longitude, radiusKm);
        return ApiResponse<List<WarehouseDto>>.SuccessResponse(warehouses.Select(MapWarehouseToDto).ToList());
    }

    public async Task<ApiResponse<WarehouseDto>> CreateWarehouseAsync(CreateWarehouseDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return ApiResponse<WarehouseDto>.ErrorResponse("Warehouse name is required");
        if (string.IsNullOrWhiteSpace(dto.Code))
            return ApiResponse<WarehouseDto>.ErrorResponse("Warehouse code is required");

        var existing = await _repository.GetWarehouseByCodeAsync(dto.Code);
        if (existing != null)
            return ApiResponse<WarehouseDto>.ErrorResponse($"Warehouse code '{dto.Code}' already exists", 409);

        if (dto.IsDefault)
        {
            var defaultWh = await _repository.GetDefaultWarehouseAsync();
            if (defaultWh != null)
            {
                defaultWh.UnsetDefault();
                await _repository.UpdateWarehouseAsync(defaultWh);
            }
        }

        var warehouse = Warehouse.Create(
            dto.Name, dto.Code, dto.Address, dto.City, dto.State,
            dto.Country, dto.PinCode, dto.Latitude, dto.Longitude, dto.IsDefault);

        warehouse.UpdateDetails(dto.Name, dto.Description, dto.Address,
            dto.ContactPerson, dto.ContactPhone, dto.ContactEmail, dto.Priority);

        if (dto.MaxCapacity.HasValue)
            warehouse.SetCapacity(dto.MaxCapacity.Value);

        await _repository.AddWarehouseAsync(warehouse);
        return ApiResponse<WarehouseDto>.SuccessResponse(MapWarehouseToDto(warehouse), "Warehouse created", 201);
    }

    public async Task<ApiResponse<WarehouseDto>> UpdateWarehouseAsync(Guid id, CreateWarehouseDto dto)
    {
        var warehouse = await _repository.GetWarehouseByIdAsync(id);
        if (warehouse == null || warehouse.IsDeleted)
            return ApiResponse<WarehouseDto>.ErrorResponse("Warehouse not found", 404);

        var existingCode = await _repository.GetWarehouseByCodeAsync(dto.Code);
        if (existingCode != null && existingCode.Id != id)
            return ApiResponse<WarehouseDto>.ErrorResponse($"Code '{dto.Code}' already exists", 409);

        if (dto.IsDefault && !warehouse.IsDefault)
        {
            var defaultWh = await _repository.GetDefaultWarehouseAsync();
            if (defaultWh != null && defaultWh.Id != id)
            {
                defaultWh.UnsetDefault();
                await _repository.UpdateWarehouseAsync(defaultWh);
            }
        }

        warehouse.UpdateDetails(dto.Name, dto.Description, dto.Address,
            dto.ContactPerson, dto.ContactPhone, dto.ContactEmail, dto.Priority);
        warehouse.UpdateGeoLocation(dto.Latitude, dto.Longitude);

        if (dto.MaxCapacity.HasValue)
            warehouse.SetCapacity(dto.MaxCapacity.Value);

        if (dto.IsDefault)
            warehouse.SetAsDefault();

        await _repository.UpdateWarehouseAsync(warehouse);
        return ApiResponse<WarehouseDto>.SuccessResponse(MapWarehouseToDto(warehouse));
    }

    public async Task<ApiResponse<bool>> DeleteWarehouseAsync(Guid id)
    {
        var warehouse = await _repository.GetWarehouseByIdAsync(id);
        if (warehouse == null || warehouse.IsDeleted)
            return ApiResponse<bool>.ErrorResponse("Warehouse not found", 404);

        var items = await _repository.GetInventoryItemsByWarehouseAsync(id);
        if (items.Any(i => i.TotalQuantity > 0))
            return ApiResponse<bool>.ErrorResponse("Cannot delete warehouse with inventory");

        warehouse.SoftDelete();
        await _repository.UpdateWarehouseAsync(warehouse);
        return ApiResponse<bool>.SuccessResponse(true);
    }

    public async Task<ApiResponse<InventoryItemDto>> GetInventoryItemByIdAsync(Guid id)
    {
        var item = await _repository.GetInventoryItemByIdAsync(id);
        if (item == null || !item.IsActive)
            return ApiResponse<InventoryItemDto>.ErrorResponse("Item not found", 404);
        return ApiResponse<InventoryItemDto>.SuccessResponse(MapInventoryItemToDto(item));
    }

    public async Task<ApiResponse<List<InventoryItemDto>>> GetInventoryItemsByWarehouseAsync(Guid warehouseId)
    {
        var warehouse = await _repository.GetWarehouseByIdAsync(warehouseId);
        if (warehouse == null || warehouse.IsDeleted)
            return ApiResponse<List<InventoryItemDto>>.ErrorResponse("Warehouse not found", 404);

        var items = await _repository.GetInventoryItemsByWarehouseAsync(warehouseId);
        return ApiResponse<List<InventoryItemDto>>.SuccessResponse(items.Where(i => i.IsActive).Select(MapInventoryItemToDto).ToList());
    }

    public async Task<ApiResponse<List<InventoryItemDto>>> GetInventoryItemsByProductAsync(Guid productId)
    {
        var items = await _repository.GetInventoryItemsByProductAsync(productId);
        return ApiResponse<List<InventoryItemDto>>.SuccessResponse(items.Where(i => i.IsActive).Select(MapInventoryItemToDto).ToList());
    }

    public async Task<ApiResponse<List<InventoryItemDto>>> GetLowStockItemsAsync(Guid? warehouseId = null)
    {
        var items = await _repository.GetLowStockItemsAsync(warehouseId);
        return ApiResponse<List<InventoryItemDto>>.SuccessResponse(items.Select(MapInventoryItemToDto).ToList());
    }

    public async Task<ApiResponse<List<InventoryItemDto>>> GetOutOfStockItemsAsync(Guid? warehouseId = null)
    {
        var items = await _repository.GetOutOfStockItemsAsync(warehouseId);
        return ApiResponse<List<InventoryItemDto>>.SuccessResponse(items.Select(MapInventoryItemToDto).ToList());
    }

    public async Task<ApiResponse<InventoryItemDto>> CreateInventoryItemAsync(Guid productId, string sku, Guid warehouseId, int initialQuantity, int lowStockThreshold = 10)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return ApiResponse<InventoryItemDto>.ErrorResponse("SKU is required");

        var warehouse = await _repository.GetWarehouseByIdAsync(warehouseId);
        if (warehouse == null || !warehouse.IsOperational)
            return ApiResponse<InventoryItemDto>.ErrorResponse("Invalid warehouse");

        var existing = await _repository.GetInventoryItemBySkuAndWarehouseAsync(sku, warehouseId);
        if (existing != null)
            return ApiResponse<InventoryItemDto>.ErrorResponse($"SKU exists in warehouse", 409);

        var item = InventoryItem.Create(productId, sku, warehouseId, initialQuantity, lowStockThreshold);
        await _repository.AddInventoryItemAsync(item);
        return ApiResponse<InventoryItemDto>.SuccessResponse(MapInventoryItemToDto(item), "Created", 201);
    }

    public async Task<ApiResponse<bool>> AdjustInventoryItemAsync(Guid id, int newQuantity, string reason, Guid userId)
    {
        var item = await _repository.GetInventoryItemByIdAsync(id);
        if (item == null || !item.IsActive)
            return ApiResponse<bool>.ErrorResponse("Item not found", 404);

        item.AdjustQuantity(newQuantity, reason);
        await _repository.UpdateInventoryItemAsync(item);

        var movement = StockMovement.Create(id, item.ProductId, item.SKU, item.WarehouseId,
            StockMovementType.ManualAdjust, Math.Abs(newQuantity - item.TotalQuantity),
            item.TotalQuantity, newQuantity, notes: reason, performedByUserId: userId);
        await _repository.AddStockMovementAsync(movement);

        return ApiResponse<bool>.SuccessResponse(true);
    }

    public async Task<ApiResponse<StockMovementDto>> AddStockAsync(StockOperationDto dto, Guid userId)
    {
        var item = await _repository.GetInventoryItemByIdAsync(dto.InventoryItemId);
        if (item == null || !item.IsActive)
            return ApiResponse<StockMovementDto>.ErrorResponse("Item not found", 404);

        var stockBefore = item.TotalQuantity;
        item.AddStock(dto.Quantity, dto.CostPrice);
        await _repository.UpdateInventoryItemAsync(item);

        var movement = StockMovement.Create(dto.InventoryItemId, item.ProductId, item.SKU, item.WarehouseId,
            StockMovementType.StockIn, dto.Quantity, stockBefore, item.TotalQuantity,
            dto.Reference, "PurchaseOrder", notes: dto.Reason, performedByUserId: userId, unitCost: dto.CostPrice);
        await _repository.AddStockMovementAsync(movement);

        return ApiResponse<StockMovementDto>.SuccessResponse(MapStockMovementToDto(movement));
    }

    public async Task<ApiResponse<StockMovementDto>> RemoveStockAsync(StockOperationDto dto, Guid userId)
    {
        var item = await _repository.GetInventoryItemByIdAsync(dto.InventoryItemId);
        if (item == null || !item.IsActive)
            return ApiResponse<StockMovementDto>.ErrorResponse("Item not found", 404);

        var stockBefore = item.TotalQuantity;
        item.RemoveStock(dto.Quantity);
        await _repository.UpdateInventoryItemAsync(item);

        var movement = StockMovement.Create(dto.InventoryItemId, item.ProductId, item.SKU, item.WarehouseId,
            StockMovementType.StockOut, dto.Quantity, stockBefore, item.TotalQuantity,
            dto.Reference, "Order", notes: dto.Reason, performedByUserId: userId);
        await _repository.AddStockMovementAsync(movement);

        return ApiResponse<StockMovementDto>.SuccessResponse(MapStockMovementToDto(movement));
    }

    public async Task<ApiResponse<StockMovementDto>> RecordDamagedStockAsync(StockOperationDto dto, Guid userId)
    {
        var item = await _repository.GetInventoryItemByIdAsync(dto.InventoryItemId);
        if (item == null || !item.IsActive)
            return ApiResponse<StockMovementDto>.ErrorResponse("Item not found", 404);

        var stockBefore = item.TotalQuantity;
        item.MarkAsDamaged(dto.Quantity);
        await _repository.UpdateInventoryItemAsync(item);

        var movement = StockMovement.Create(dto.InventoryItemId, item.ProductId, item.SKU, item.WarehouseId,
            StockMovementType.Damage, dto.Quantity, stockBefore, item.TotalQuantity,
            dto.Reference, notes: dto.Reason, performedByUserId: userId);
        await _repository.AddStockMovementAsync(movement);

        return ApiResponse<StockMovementDto>.SuccessResponse(MapStockMovementToDto(movement));
    }

    public async Task<ApiResponse<List<StockMovementDto>>> GetStockMovementsAsync(Guid inventoryItemId)
    {
        var movements = await _repository.GetStockMovementsByInventoryItemAsync(inventoryItemId);
        return ApiResponse<List<StockMovementDto>>.SuccessResponse(movements.Select(MapStockMovementToDto).ToList());
    }

    public async Task<ApiResponse<StockReservationDto>> ReserveStockAsync(ReserveStockDto dto)
    {
        var item = await _repository.GetInventoryItemByIdAsync(dto.InventoryItemId);
        if (item == null || !item.IsActive)
            return ApiResponse<StockReservationDto>.ErrorResponse("Item not found", 404);

        if (dto.Quantity > item.AvailableQuantity)
            return ApiResponse<StockReservationDto>.ErrorResponse("Insufficient stock");

        var stockBefore = item.TotalQuantity;
        item.Reserve(dto.Quantity);
        await _repository.UpdateInventoryItemAsync(item);

        var reservation = StockReservation.Create(dto.InventoryItemId, item.ProductId, item.SKU,
            item.WarehouseId, $"ORD-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
            Guid.NewGuid(), dto.Quantity, dto.ExpirationMinutes);
        await _repository.AddStockReservationAsync(reservation);

        var movement = StockMovement.Create(dto.InventoryItemId, item.ProductId, item.SKU, item.WarehouseId,
            StockMovementType.Reserve, dto.Quantity, stockBefore, item.TotalQuantity,
            reservation.OrderId, "Order", performedByUserId: reservation.UserId);
        await _repository.AddStockMovementAsync(movement);

        return ApiResponse<StockReservationDto>.SuccessResponse(MapReservationToDto(reservation), "Reserved", 201);
    }

    public async Task<ApiResponse<StockReservationDto>> GetReservationByCodeAsync(string reservationCode)
    {
        var reservation = await _repository.GetStockReservationByCodeAsync(reservationCode);
        if (reservation == null)
            return ApiResponse<StockReservationDto>.ErrorResponse("Not found", 404);
        return ApiResponse<StockReservationDto>.SuccessResponse(MapReservationToDto(reservation));
    }

    public async Task<ApiResponse<bool>> ConfirmReservationAsync(Guid reservationId, Guid userId)
    {
        var reservation = await _repository.GetStockReservationByIdAsync(reservationId);
        if (reservation == null)
            return ApiResponse<bool>.ErrorResponse("Not found", 404);

        if (reservation.Status != StockReservationStatus.Pending)
            return ApiResponse<bool>.ErrorResponse("Not in pending state");

        if (reservation.IsExpired)
        {
            reservation.Expire();
            await _repository.UpdateStockReservationAsync(reservation);
            var item = await _repository.GetInventoryItemByIdAsync(reservation.InventoryItemId);
            if (item != null)
            {
                item.Release(reservation.Quantity);
                await _repository.UpdateInventoryItemAsync(item);
            }
            return ApiResponse<bool>.ErrorResponse("Reservation expired");
        }

        var inventoryItem = await _repository.GetInventoryItemByIdAsync(reservation.InventoryItemId);
        if (inventoryItem == null)
            return ApiResponse<bool>.ErrorResponse("Item not found", 404);

        var stockBefore = inventoryItem.TotalQuantity;
        inventoryItem.ConfirmReservation(reservation.Quantity);
        await _repository.UpdateInventoryItemAsync(inventoryItem);

        reservation.Confirm();
        await _repository.UpdateStockReservationAsync(reservation);

        var movement = StockMovement.Create(reservation.InventoryItemId, inventoryItem.ProductId,
            inventoryItem.SKU, inventoryItem.WarehouseId, StockMovementType.StockOut,
            reservation.Quantity, stockBefore, inventoryItem.TotalQuantity,
            reservation.OrderId, "Order", performedByUserId: userId);
        await _repository.AddStockMovementAsync(movement);

        return ApiResponse<bool>.SuccessResponse(true, "Confirmed");
    }

    public async Task<ApiResponse<bool>> ReleaseReservationAsync(Guid reservationId)
    {
        var reservation = await _repository.GetStockReservationByIdAsync(reservationId);
        if (reservation == null)
            return ApiResponse<bool>.ErrorResponse("Not found", 404);

        if (reservation.Status != StockReservationStatus.Pending)
            return ApiResponse<bool>.ErrorResponse("Not in pending state");

        var item = await _repository.GetInventoryItemByIdAsync(reservation.InventoryItemId);
        if (item != null)
        {
            var stockBefore = item.TotalQuantity;
            item.Release(reservation.Quantity);
            await _repository.UpdateInventoryItemAsync(item);

            var movement = StockMovement.Create(reservation.InventoryItemId, item.ProductId,
                item.SKU, item.WarehouseId, StockMovementType.Release, reservation.Quantity,
                stockBefore, item.TotalQuantity, reservation.OrderId, "Order");
            await _repository.AddStockMovementAsync(movement);
        }

        reservation.Release("Released by user");
        await _repository.UpdateStockReservationAsync(reservation);

        return ApiResponse<bool>.SuccessResponse(true, "Released");
    }

    public async Task<ApiResponse<bool>> ProcessExpiredReservationsAsync()
    {
        var expired = await _repository.GetExpiredReservationsAsync();
        foreach (var reservation in expired)
        {
            reservation.Expire();
            await _repository.UpdateStockReservationAsync(reservation);

            var item = await _repository.GetInventoryItemByIdAsync(reservation.InventoryItemId);
            if (item != null)
            {
                item.Release(reservation.Quantity);
                await _repository.UpdateInventoryItemAsync(item);
            }
        }
        return ApiResponse<bool>.SuccessResponse(true, $"Processed {expired.Count}");
    }

    public async Task<ApiResponse<StockTransferDto>> CreateStockTransferAsync(CreateStockTransferDto dto, Guid userId)
    {
        if (dto.FromWarehouseId == dto.ToWarehouseId)
            return ApiResponse<StockTransferDto>.ErrorResponse("Same warehouse");

        var sourceItem = await _repository.GetInventoryItemBySkuAndWarehouseAsync(dto.SKU, dto.FromWarehouseId);
        if (sourceItem == null || sourceItem.AvailableQuantity < dto.Quantity)
            return ApiResponse<StockTransferDto>.ErrorResponse("Insufficient stock");

        sourceItem.Reserve(dto.Quantity);
        await _repository.UpdateInventoryItemAsync(sourceItem);

        var transfer = StockTransfer.Create(dto.FromWarehouseId, dto.ToWarehouseId,
            sourceItem.ProductId, dto.SKU, dto.Quantity, userId, dto.Notes);
        await _repository.AddStockTransferAsync(transfer);

        var fromWh = await _repository.GetWarehouseByIdAsync(dto.FromWarehouseId);
        var toWh = await _repository.GetWarehouseByIdAsync(dto.ToWarehouseId);

        return ApiResponse<StockTransferDto>.SuccessResponse(MapTransferToDto(transfer, fromWh!, toWh!), "Created", 201);
    }

    public async Task<ApiResponse<StockTransferDto>> ShipTransferAsync(Guid transferId, Guid userId)
    {
        var transfer = await _repository.GetStockTransferByIdAsync(transferId);
        if (transfer == null)
            return ApiResponse<StockTransferDto>.ErrorResponse("Not found", 404);

        if (transfer.Status != "Pending")
            return ApiResponse<StockTransferDto>.ErrorResponse("Not pending");

        transfer.Ship();
        await _repository.UpdateStockTransferAsync(transfer);

        var sourceItem = await _repository.GetInventoryItemBySkuAndWarehouseAsync(transfer.SKU, transfer.SourceWarehouseId);
        if (sourceItem != null)
        {
            sourceItem.Release(transfer.Quantity);
            sourceItem.RemoveStock(transfer.Quantity);
            await _repository.UpdateInventoryItemAsync(sourceItem);

            var movement = StockMovement.Create(sourceItem.Id, sourceItem.ProductId, sourceItem.SKU,
                sourceItem.WarehouseId, StockMovementType.WarehouseTransfer, transfer.Quantity,
                sourceItem.TotalQuantity + transfer.Quantity, sourceItem.TotalQuantity,
                transfer.TransferNumber, "Transfer", performedByUserId: userId,
                destinationWarehouseId: transfer.DestinationWarehouseId);
            await _repository.AddStockMovementAsync(movement);
        }

        var fromWh = await _repository.GetWarehouseByIdAsync(transfer.SourceWarehouseId);
        var toWh = await _repository.GetWarehouseByIdAsync(transfer.DestinationWarehouseId);
        return ApiResponse<StockTransferDto>.SuccessResponse(MapTransferToDto(transfer, fromWh!, toWh!));
    }

    public async Task<ApiResponse<StockTransferDto>> ReceiveTransferAsync(Guid transferId, Guid userId)
    {
        var transfer = await _repository.GetStockTransferByIdAsync(transferId);
        if (transfer == null)
            return ApiResponse<StockTransferDto>.ErrorResponse("Not found", 404);

        if (transfer.Status != "InTransit")
            return ApiResponse<StockTransferDto>.ErrorResponse("Not in transit");

        transfer.Complete(userId);
        await _repository.UpdateStockTransferAsync(transfer);

        var destItem = await _repository.GetInventoryItemBySkuAndWarehouseAsync(transfer.SKU, transfer.DestinationWarehouseId);
        if (destItem != null)
        {
            destItem.AddStock(transfer.Quantity);
            await _repository.UpdateInventoryItemAsync(destItem);

            var movement = StockMovement.Create(destItem.Id, destItem.ProductId, destItem.SKU,
                destItem.WarehouseId, StockMovementType.WarehouseTransfer, transfer.Quantity,
                destItem.TotalQuantity - transfer.Quantity, destItem.TotalQuantity,
                transfer.TransferNumber, "Transfer", performedByUserId: userId);
            await _repository.AddStockMovementAsync(movement);
        }
        else
        {
            var newItem = InventoryItem.Create(transfer.ProductId, transfer.SKU, transfer.DestinationWarehouseId, transfer.Quantity);
            await _repository.AddInventoryItemAsync(newItem);

            var movement = StockMovement.Create(newItem.Id, newItem.ProductId, newItem.SKU,
                newItem.WarehouseId, StockMovementType.WarehouseTransfer, transfer.Quantity,
                0, transfer.Quantity, transfer.TransferNumber, "Transfer", performedByUserId: userId);
            await _repository.AddStockMovementAsync(movement);
        }

        var fromWh = await _repository.GetWarehouseByIdAsync(transfer.SourceWarehouseId);
        var toWh = await _repository.GetWarehouseByIdAsync(transfer.DestinationWarehouseId);
        return ApiResponse<StockTransferDto>.SuccessResponse(MapTransferToDto(transfer, fromWh!, toWh!));
    }

    public async Task<ApiResponse<StockTransferDto>> CancelTransferAsync(Guid transferId, Guid userId)
    {
        var transfer = await _repository.GetStockTransferByIdAsync(transferId);
        if (transfer == null)
            return ApiResponse<StockTransferDto>.ErrorResponse("Not found", 404);

        if (transfer.Status == "Completed" || transfer.Status == "Cancelled")
            return ApiResponse<StockTransferDto>.ErrorResponse("Cannot cancel");

        if (transfer.Status == "InTransit")
        {
            var sourceItem = await _repository.GetInventoryItemBySkuAndWarehouseAsync(transfer.SKU, transfer.SourceWarehouseId);
            if (sourceItem != null)
            {
                sourceItem.AddStock(transfer.Quantity);
                await _repository.UpdateInventoryItemAsync(sourceItem);
            }
        }
        else
        {
            var sourceItem = await _repository.GetInventoryItemBySkuAndWarehouseAsync(transfer.SKU, transfer.SourceWarehouseId);
            if (sourceItem != null)
            {
                sourceItem.Release(transfer.Quantity);
                await _repository.UpdateInventoryItemAsync(sourceItem);
            }
        }

        transfer.Cancel("Cancelled by user");
        await _repository.UpdateStockTransferAsync(transfer);

        var fromWh = await _repository.GetWarehouseByIdAsync(transfer.SourceWarehouseId);
        var toWh = await _repository.GetWarehouseByIdAsync(transfer.DestinationWarehouseId);
        return ApiResponse<StockTransferDto>.SuccessResponse(MapTransferToDto(transfer, fromWh!, toWh!));
    }

    public async Task<ApiResponse<List<StockTransferDto>>> GetPendingTransfersAsync()
    {
        var transfers = await _repository.GetPendingTransfersAsync();
        var dtos = new List<StockTransferDto>();
        foreach (var t in transfers)
        {
            var fromWh = await _repository.GetWarehouseByIdAsync(t.SourceWarehouseId);
            var toWh = await _repository.GetWarehouseByIdAsync(t.DestinationWarehouseId);
            dtos.Add(MapTransferToDto(t, fromWh!, toWh!));
        }
        return ApiResponse<List<StockTransferDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<StockTransferDto>> GetTransferByIdAsync(Guid id)
    {
        var transfer = await _repository.GetStockTransferByIdAsync(id);
        if (transfer == null)
            return ApiResponse<StockTransferDto>.ErrorResponse("Not found", 404);

        var fromWh = await _repository.GetWarehouseByIdAsync(transfer.SourceWarehouseId);
        var toWh = await _repository.GetWarehouseByIdAsync(transfer.DestinationWarehouseId);
        return ApiResponse<StockTransferDto>.SuccessResponse(MapTransferToDto(transfer, fromWh!, toWh!));
    }

    public async Task<ApiResponse<List<InventoryAlertDto>>> GetAlertsAsync(Guid? warehouseId = null, InventoryAlertType? type = null)
    {
        var alerts = await _repository.GetUnresolvedAlertsAsync(warehouseId, type);
        return ApiResponse<List<InventoryAlertDto>>.SuccessResponse(alerts.Select(MapAlertToDto).ToList());
    }

    public async Task<ApiResponse<bool>> MarkAlertAsReadAsync(Guid alertId)
    {
        var alert = await _repository.GetInventoryAlertByIdAsync(alertId);
        if (alert == null)
            return ApiResponse<bool>.ErrorResponse("Not found", 404);

        alert.MarkNotificationSent();
        await _repository.UpdateInventoryAlertAsync(alert);
        return ApiResponse<bool>.SuccessResponse(true);
    }

    public async Task<ApiResponse<bool>> ResolveAlertAsync(Guid alertId)
    {
        var alert = await _repository.GetInventoryAlertByIdAsync(alertId);
        if (alert == null)
            return ApiResponse<bool>.ErrorResponse("Not found", 404);

        alert.Resolve();
        await _repository.UpdateInventoryAlertAsync(alert);
        return ApiResponse<bool>.SuccessResponse(true);
    }

    public async Task<ApiResponse<bool>> GenerateStockAlertsAsync()
    {
        var lowStock = await _repository.GetLowStockItemsAsync();
        var outOfStock = await _repository.GetOutOfStockItemsAsync();

        foreach (var item in lowStock.Where(i => i.IsLowStock))
        {
            var exists = (await _repository.GetAlertsByInventoryItemAsync(item.Id))
                .Any(a => a.Type == InventoryAlertType.LowStock && !a.IsResolved);
            if (!exists)
            {
                var alert = InventoryAlert.Create(item.Id, item.ProductId, item.SKU, item.WarehouseId,
                    InventoryAlertType.LowStock, item.AvailableQuantity, item.LowStockThreshold);
                await _repository.AddInventoryAlertAsync(alert);
            }
        }

        foreach (var item in outOfStock)
        {
            var exists = (await _repository.GetAlertsByInventoryItemAsync(item.Id))
                .Any(a => a.Type == InventoryAlertType.OutOfStock && !a.IsResolved);
            if (!exists)
            {
                var alert = InventoryAlert.Create(item.Id, item.ProductId, item.SKU, item.WarehouseId,
                    InventoryAlertType.OutOfStock, 0, item.LowStockThreshold);
                await _repository.AddInventoryAlertAsync(alert);
            }
        }

        return ApiResponse<bool>.SuccessResponse(true);
    }

    private static WarehouseDto MapWarehouseToDto(Warehouse w)
    {
        return new WarehouseDto
        {
            Id = w.Id, Name = w.Name, Code = w.Code, Description = w.Description,
            Address = w.Address, City = w.City, State = w.State, Country = w.Country,
            PinCode = w.PinCode, Latitude = w.Latitude, Longitude = w.Longitude,
            ContactPerson = w.ContactPerson, ContactPhone = w.ContactPhone, ContactEmail = w.ContactEmail,
            MaxCapacity = w.MaxCapacity, Status = w.Status, IsDefault = w.IsDefault, Priority = w.Priority,
            TotalItems = w.InventoryItems.Count(i => i.IsActive), CreatedAt = w.CreatedAt
        };
    }

    private static InventoryItemDto MapInventoryItemToDto(InventoryItem i)
    {
        return new InventoryItemDto
        {
            Id = i.Id, ProductId = i.ProductId, SKU = i.SKU, WarehouseId = i.WarehouseId,
            WarehouseName = i.Warehouse?.Name ?? "", TotalQuantity = i.TotalQuantity,
            ReservedQuantity = i.ReservedQuantity, AvailableQuantity = i.AvailableQuantity,
            DamagedQuantity = i.DamagedQuantity, LowStockThreshold = i.LowStockThreshold,
            IsLowStock = i.IsLowStock, IsOutOfStock = i.IsOutOfStock, AverageCost = i.AverageCost,
            ExpiryDate = i.ExpiryDate, Location = i.Location, CreatedAt = i.CreatedAt, UpdatedAt = i.UpdatedAt
        };
    }

    private static StockMovementDto MapStockMovementToDto(StockMovement m)
    {
        return new StockMovementDto
        {
            Id = m.Id, InventoryItemId = m.InventoryItemId, WarehouseId = m.WarehouseId,
            Type = m.Type, Quantity = m.Quantity, Reference = m.ReferenceId ?? "",
            Reason = m.Reason, CreatedAt = m.CreatedAt
        };
    }

    private static StockReservationDto MapReservationToDto(StockReservation r)
    {
        return new StockReservationDto
        {
            Id = r.Id, InventoryItemId = r.InventoryItemId, ReservationCode = r.OrderId,
            Quantity = r.Quantity, Status = r.Status, ExpiresAt = r.ExpiresAt,
            ConfirmedAt = r.ConfirmedAt, CreatedAt = r.CreatedAt
        };
    }

    private static StockTransferDto MapTransferToDto(StockTransfer t, Warehouse fromWh, Warehouse toWh)
    {
        return new StockTransferDto
        {
            Id = t.Id, FromWarehouseId = t.SourceWarehouseId, FromWarehouseName = fromWh.Name,
            ToWarehouseId = t.DestinationWarehouseId, ToWarehouseName = toWh.Name,
            SKU = t.SKU, Quantity = t.Quantity, Status = Enum.Parse<StockTransferStatus>(t.Status),
            Reference = t.TransferNumber, ShippedAt = t.ShippedAt, ReceivedAt = t.ActualArrival,
            CreatedAt = t.CreatedAt
        };
    }

    private static InventoryAlertDto MapAlertToDto(InventoryAlert a)
    {
        return new InventoryAlertDto
        {
            Id = a.Id, InventoryItemId = a.InventoryItemId, WarehouseId = a.WarehouseId,
            Type = a.Type, Message = a.Message, IsRead = a.IsNotificationSent,
            ResolvedAt = a.ResolvedAt, CreatedAt = a.CreatedAt
        };
    }
}
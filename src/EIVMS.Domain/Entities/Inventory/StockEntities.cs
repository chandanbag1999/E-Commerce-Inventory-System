using EIVMS.Domain.Common;
using EIVMS.Domain.Enums.Inventory;

namespace EIVMS.Domain.Entities.Inventory;

public class StockMovement : BaseEntity
{
    public Guid InventoryItemId { get; private set; }
    public Guid ProductId { get; private set; }
    public string SKU { get; private set; } = string.Empty;
    public Guid WarehouseId { get; private set; }

    public StockMovementType Type { get; private set; }
    public int Quantity { get; private set; }

    public int StockBefore { get; private set; }
    public int StockAfter { get; private set; }

    public string? ReferenceId { get; private set; }
    public string? ReferenceType { get; private set; }

    public string? Notes { get; private set; }
    public string? Reason { get; private set; }

    public decimal? UnitCost { get; private set; }
    public decimal? TotalCost { get; private set; }

    public Guid? PerformedByUserId { get; private set; }
    public string? PerformedByName { get; private set; }

    public Guid? DestinationWarehouseId { get; private set; }

    public InventoryItem? InventoryItem { get; private set; } = null!;
    public Warehouse? Warehouse { get; private set; } = null!;

    private StockMovement() { }

    public static StockMovement Create(
        Guid inventoryItemId,
        Guid productId,
        string sku,
        Guid warehouseId,
        StockMovementType type,
        int quantity,
        int stockBefore,
        int stockAfter,
        string? referenceId = null,
        string? referenceType = null,
        string? notes = null,
        string? reason = null,
        Guid? performedByUserId = null,
        string? performedByName = null,
        decimal? unitCost = null,
        Guid? destinationWarehouseId = null)
    {
        return new StockMovement
        {
            Id = Guid.NewGuid(),
            InventoryItemId = inventoryItemId,
            ProductId = productId,
            SKU = sku.ToUpperInvariant(),
            WarehouseId = warehouseId,
            Type = type,
            Quantity = quantity,
            StockBefore = stockBefore,
            StockAfter = stockAfter,
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            Notes = notes,
            Reason = reason,
            PerformedByUserId = performedByUserId,
            PerformedByName = performedByName,
            UnitCost = unitCost,
            TotalCost = unitCost.HasValue ? unitCost.Value * quantity : null,
            DestinationWarehouseId = destinationWarehouseId,
            CreatedAt = DateTime.UtcNow
        };
    }
}

public class StockReservation : BaseEntity
{
    public Guid InventoryItemId { get; private set; }
    public Guid ProductId { get; private set; }
    public string SKU { get; private set; } = string.Empty;
    public Guid WarehouseId { get; private set; }

    public string OrderId { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }

    public int Quantity { get; private set; }
    public StockReservationStatus Status { get; private set; } = StockReservationStatus.Pending;

    public DateTime ExpiresAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? ReleasedAt { get; private set; }

    public decimal? UnitPrice { get; private set; }
    public decimal? TotalPrice { get; private set; }

    public string? ReleaseReason { get; private set; }

    public InventoryItem? InventoryItem { get; private set; } = null!;

    private StockReservation() { }

    public static StockReservation Create(
        Guid inventoryItemId,
        Guid productId,
        string sku,
        Guid warehouseId,
        string orderId,
        Guid userId,
        int quantity,
        int expiryMinutes = 30,
        decimal? unitPrice = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Reservation quantity must be positive");

        return new StockReservation
        {
            Id = Guid.NewGuid(),
            InventoryItemId = inventoryItemId,
            ProductId = productId,
            SKU = sku.ToUpperInvariant(),
            WarehouseId = warehouseId,
            OrderId = orderId,
            UserId = userId,
            Quantity = quantity,
            Status = StockReservationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
            UnitPrice = unitPrice,
            TotalPrice = unitPrice.HasValue ? unitPrice.Value * quantity : null,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Confirm()
    {
        if (Status != StockReservationStatus.Pending)
            throw new InvalidOperationException($"Cannot confirm reservation in {Status} status");

        if (IsExpired)
            throw new InvalidOperationException("Reservation has expired");

        Status = StockReservationStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void Release(string reason)
    {
        if (Status == StockReservationStatus.Confirmed)
            throw new InvalidOperationException("Cannot release already confirmed reservation");

        if (Status == StockReservationStatus.Released)
            return;

        Status = StockReservationStatus.Released;
        ReleasedAt = DateTime.UtcNow;
        ReleaseReason = reason;
        SetUpdatedAt();
    }

    public void Expire()
    {
        if (Status != StockReservationStatus.Pending)
            return;

        Status = StockReservationStatus.Expired;
        ReleasedAt = DateTime.UtcNow;
        ReleaseReason = "Reservation expired due to timeout";
        SetUpdatedAt();
    }

    public bool IsExpired =>
        Status == StockReservationStatus.Pending &&
        DateTime.UtcNow > ExpiresAt;

    public bool IsActive =>
        Status == StockReservationStatus.Pending && !IsExpired;
}

public class StockTransfer : BaseEntity
{
    public Guid SourceWarehouseId { get; private set; }
    public Guid DestinationWarehouseId { get; private set; }
    public Guid ProductId { get; private set; }
    public string SKU { get; private set; } = string.Empty;

    public int Quantity { get; private set; }
    public string TransferNumber { get; private set; } = string.Empty;

    public string Status { get; private set; } = "Pending";

    public string? Notes { get; private set; }
    public DateTime? ExpectedArrival { get; private set; }
    public DateTime? ActualArrival { get; private set; }
    public DateTime? ShippedAt { get; private set; }

    public Guid InitiatedByUserId { get; private set; }
    public Guid? ReceivedByUserId { get; private set; }

    public Warehouse SourceWarehouse { get; private set; } = null!;
    public Warehouse DestinationWarehouse { get; private set; } = null!;

    private StockTransfer() { }

    public static StockTransfer Create(
        Guid sourceWarehouseId,
        Guid destinationWarehouseId,
        Guid productId,
        string sku,
        int quantity,
        Guid initiatedByUserId,
        string? notes = null)
    {
        if (sourceWarehouseId == destinationWarehouseId)
            throw new ArgumentException("Source and destination warehouses must be different");

        if (quantity <= 0)
            throw new ArgumentException("Transfer quantity must be positive");

        return new StockTransfer
        {
            Id = Guid.NewGuid(),
            SourceWarehouseId = sourceWarehouseId,
            DestinationWarehouseId = destinationWarehouseId,
            ProductId = productId,
            SKU = sku.ToUpperInvariant(),
            Quantity = quantity,
            TransferNumber = GenerateTransferNumber(),
            Status = "Pending",
            Notes = notes,
            InitiatedByUserId = initiatedByUserId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Ship(DateTime? expectedArrival = null)
    {
        if (Status != "Pending")
            throw new InvalidOperationException("Can only ship pending transfers");

        Status = "InTransit";
        ShippedAt = DateTime.UtcNow;
        ExpectedArrival = expectedArrival;
        SetUpdatedAt();
    }

    public void Complete(Guid receivedByUserId)
    {
        if (Status != "InTransit")
            throw new InvalidOperationException("Can only complete in-transit transfers");

        Status = "Completed";
        ActualArrival = DateTime.UtcNow;
        ReceivedByUserId = receivedByUserId;
        SetUpdatedAt();
    }

    public void Cancel(string reason)
    {
        if (Status == "Completed")
            throw new InvalidOperationException("Cannot cancel completed transfer");

        Status = "Cancelled";
        Notes = string.IsNullOrEmpty(Notes) 
            ? $"Cancellation reason: {reason}" 
            : $"{Notes}\nCancellation reason: {reason}";
        SetUpdatedAt();
    }

    private static string GenerateTransferNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = new Random().Next(1000, 9999);
        return $"TRF-{timestamp}-{random}";
    }
}

public class InventoryAlert : BaseEntity
{
    public Guid InventoryItemId { get; private set; }
    public Guid ProductId { get; private set; }
    public string SKU { get; private set; } = string.Empty;
    public Guid WarehouseId { get; private set; }
    public InventoryAlertType Type { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public int CurrentStock { get; private set; }
    public int ThresholdValue { get; private set; }
    public bool IsResolved { get; private set; } = false;
    public DateTime? ResolvedAt { get; private set; }
    public bool IsNotificationSent { get; private set; } = false;

    private InventoryAlert() { }

    public static InventoryAlert Create(
        Guid inventoryItemId,
        Guid productId,
        string sku,
        Guid warehouseId,
        InventoryAlertType type,
        int currentStock,
        int thresholdValue)
    {
        var message = type switch
        {
            InventoryAlertType.LowStock =>
                $"Low stock alert: {sku} has only {currentStock} units (threshold: {thresholdValue})",
            InventoryAlertType.OutOfStock =>
                $"Out of stock: {sku} is completely out of stock",
            InventoryAlertType.Overstock =>
                $"Overstock alert: {sku} has {currentStock} units (max: {thresholdValue})",
            _ => $"Inventory alert for {sku}"
        };

        return new InventoryAlert
        {
            Id = Guid.NewGuid(),
            InventoryItemId = inventoryItemId,
            ProductId = productId,
            SKU = sku,
            WarehouseId = warehouseId,
            Type = type,
            Message = message,
            CurrentStock = currentStock,
            ThresholdValue = thresholdValue,
            IsResolved = false,
            IsNotificationSent = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkNotificationSent()
    {
        IsNotificationSent = true;
        SetUpdatedAt();
    }

    public void Resolve()
    {
        IsResolved = true;
        ResolvedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }
}
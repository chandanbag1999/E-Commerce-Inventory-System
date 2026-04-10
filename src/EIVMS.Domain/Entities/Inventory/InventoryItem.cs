using EIVMS.Domain.Common;

namespace EIVMS.Domain.Entities.Inventory;

public class InventoryItem : BaseEntity
{
    public Guid ProductId { get; private set; }
    public string SKU { get; private set; } = string.Empty;
    public Guid WarehouseId { get; private set; }

    public int TotalQuantity { get; private set; } = 0;
    public int ReservedQuantity { get; private set; } = 0;
    public int DamagedQuantity { get; private set; } = 0;
    public int InTransitQuantity { get; private set; } = 0;

    public int LowStockThreshold { get; private set; } = 10;
    public int? MaxStockLevel { get; private set; }
    public int ReorderPoint { get; private set; } = 5;
    public int? ReorderQuantity { get; private set; }

    public decimal? AverageCost { get; private set; }
    public decimal? LastCost { get; private set; }

    public int Version { get; private set; } = 0;

    public DateTime? ExpiryDate { get; private set; }
    public string? BatchNumber { get; private set; }
    public string? Location { get; private set; }

    public bool IsActive { get; private set; } = true;

    public Warehouse Warehouse { get; private set; } = null!;

    private InventoryItem() { }

    public static InventoryItem Create(
        Guid productId,
        string sku,
        Guid warehouseId,
        int initialQuantity = 0,
        int lowStockThreshold = 10)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required");
        if (initialQuantity < 0)
            throw new ArgumentException("Initial quantity cannot be negative");

        return new InventoryItem
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            SKU = sku.ToUpperInvariant().Trim(),
            WarehouseId = warehouseId,
            TotalQuantity = initialQuantity,
            ReservedQuantity = 0,
            DamagedQuantity = 0,
            LowStockThreshold = lowStockThreshold,
            ReorderPoint = Math.Max(1, lowStockThreshold / 2),
            Version = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public int AvailableQuantity => Math.Max(0, TotalQuantity - ReservedQuantity - DamagedQuantity);
    public bool IsLowStock => AvailableQuantity <= LowStockThreshold && AvailableQuantity > 0;
    public bool IsOutOfStock => AvailableQuantity <= 0;
    public bool IsOverstock => MaxStockLevel.HasValue && TotalQuantity > MaxStockLevel.Value;
    public bool IsNearExpiry => ExpiryDate.HasValue && ExpiryDate.Value <= DateTime.UtcNow.AddDays(7) && ExpiryDate.Value > DateTime.UtcNow;

    public void AddStock(int quantity, decimal? costPrice = null)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive");
        TotalQuantity += quantity;
        if (costPrice.HasValue)
        {
            if (AverageCost.HasValue && TotalQuantity > quantity)
            {
                var oldTotal = TotalQuantity - quantity;
                AverageCost = ((AverageCost.Value * oldTotal) + (costPrice.Value * quantity)) / TotalQuantity;
            }
            else AverageCost = costPrice;
            LastCost = costPrice;
        }
        IncrementVersion();
        SetUpdatedAt();
    }

    public void RemoveStock(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive");
        if (quantity > AvailableQuantity)
            throw new InvalidOperationException($"Insufficient stock. Available: {AvailableQuantity}");
        TotalQuantity -= quantity;
        IncrementVersion();
        SetUpdatedAt();
    }

    public void Reserve(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Reserve quantity must be positive");
        if (quantity > AvailableQuantity)
            throw new InvalidOperationException($"Cannot reserve {quantity}. Available: {AvailableQuantity}");
        ReservedQuantity += quantity;
        IncrementVersion();
        SetUpdatedAt();
    }

    public void Release(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Release quantity must be positive");
        if (quantity > ReservedQuantity)
            throw new InvalidOperationException($"Cannot release {quantity}. Only {ReservedQuantity} reserved.");
        ReservedQuantity -= quantity;
        IncrementVersion();
        SetUpdatedAt();
    }

    public void ConfirmReservation(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Confirm quantity must be positive");
        if (quantity > ReservedQuantity)
            throw new InvalidOperationException($"Cannot confirm {quantity}. Only {ReservedQuantity} reserved.");
        ReservedQuantity -= quantity;
        TotalQuantity -= quantity;
        IncrementVersion();
        SetUpdatedAt();
    }

    public void MarkAsDamaged(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Damage quantity must be positive");
        if (quantity > AvailableQuantity)
            throw new InvalidOperationException($"Cannot mark {quantity} as damaged. Available: {AvailableQuantity}");
        DamagedQuantity += quantity;
        IncrementVersion();
        SetUpdatedAt();
    }

    public void AdjustQuantity(int newQuantity, string reason)
    {
        if (newQuantity < 0) throw new ArgumentException("Quantity cannot be negative");
        TotalQuantity = newQuantity;
        IncrementVersion();
        SetUpdatedAt();
    }

    public void UpdateThresholds(int lowStockThreshold, int? maxStockLevel, int reorderPoint, int? reorderQuantity)
    {
        LowStockThreshold = Math.Max(0, lowStockThreshold);
        MaxStockLevel = maxStockLevel;
        ReorderPoint = Math.Max(0, reorderPoint);
        ReorderQuantity = reorderQuantity;
        SetUpdatedAt();
    }

    public void SetExpiry(DateTime? expiryDate)
    {
        ExpiryDate = expiryDate;
        SetUpdatedAt();
    }

    private void IncrementVersion() => Version++;
}
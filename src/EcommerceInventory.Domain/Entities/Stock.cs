using EcommerceInventory.Domain.Common;
using EcommerceInventory.Domain.Exceptions;

namespace EcommerceInventory.Domain.Entities;

/// <summary>
/// Stock entity - tracks inventory levels per product per warehouse
/// </summary>
public class Stock : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public int Quantity { get; set; } = 0;
    public int ReservedQty { get; set; } = 0;
    public DateTime? LastCountedAt { get; set; }

    // Navigation properties
    public Product Product { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
    public ICollection<StockMovement> Movements { get; set; } = new List<StockMovement>();

    /// <summary>
    /// Computed property - available quantity (total - reserved)
    /// </summary>
    public int AvailableQty => Quantity - ReservedQty;

    /// <summary>
    /// Factory method to create a new stock record
    /// </summary>
    public static Stock Create(Guid productId, Guid warehouseId, int initialQty = 0)
    {
        return new Stock
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            Quantity = initialQty,
            ReservedQty = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Adds stock to this location (e.g., from purchase order receive)
    /// </summary>
    public StockMovement AddStock(
        int qty,
        string movementType,
        Guid? referenceId,
        string referenceType,
        string? notes,
        Guid performedBy)
    {
        if (qty <= 0)
            throw new DomainException("Quantity must be greater than 0");

        var quantityBefore = Quantity;
        Quantity += qty;
        UpdatedAt = DateTime.UtcNow;

        return StockMovement.Create(
            Id,
            movementType,
            qty,
            quantityBefore,
            Quantity,
            referenceId,
            referenceType,
            notes,
            performedBy);
    }

    /// <summary>
    /// Removes stock from this location (e.g., sales order ship)
    /// </summary>
    public StockMovement RemoveStock(
        int qty,
        string movementType,
        Guid? referenceId,
        string referenceType,
        string? notes,
        Guid performedBy)
    {
        if (qty <= 0)
            throw new DomainException("Quantity must be greater than 0");

        if (Quantity < qty)
            throw new BusinessRuleViolationException(
                $"Insufficient stock. Available: {Quantity}, Requested: {qty}");

        var quantityBefore = Quantity;
        Quantity -= qty;
        UpdatedAt = DateTime.UtcNow;

        return StockMovement.Create(
            Id,
            movementType,
            qty,
            quantityBefore,
            Quantity,
            referenceId,
            referenceType,
            notes,
            performedBy);
    }

    /// <summary>
    /// Reserves stock for a sales order (reduces available qty but not total)
    /// </summary>
    public void Reserve(int qty)
    {
        if (qty <= 0)
            throw new DomainException("Quantity must be greater than 0");

        if (AvailableQty < qty)
            throw new BusinessRuleViolationException(
                $"Insufficient available stock for reservation. Available: {AvailableQty}, Required: {qty}");

        ReservedQty += qty;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Releases a reservation (e.g., cancelled order)
    /// </summary>
    public void ReleaseReservation(int qty)
    {
        if (qty <= 0)
            throw new DomainException("Quantity must be greater than 0");

        if (ReservedQty < qty)
            throw new BusinessRuleViolationException(
                $"Cannot release more than reserved. Reserved: {ReservedQty}, Requested: {qty}");

        ReservedQty -= qty;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if stock is below reorder level
    /// </summary>
    public bool IsLowStock(int reorderLevel)
    {
        return AvailableQty <= reorderLevel;
    }
}

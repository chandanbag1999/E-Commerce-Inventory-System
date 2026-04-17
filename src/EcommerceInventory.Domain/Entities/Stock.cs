using EcommerceInventory.Domain.Common;
using EcommerceInventory.Domain.Enums;
using EcommerceInventory.Domain.Exceptions;

namespace EcommerceInventory.Domain.Entities;

public class Stock : BaseEntity
{
    public Guid      ProductId      { get; private set; }
    public Guid      WarehouseId    { get; private set; }
    public int       Quantity       { get; private set; } = 0;
    public int       ReservedQty    { get; private set; } = 0;
    public int       AvailableQty   => Quantity - ReservedQty;
    public DateTime? LastCountedAt  { get; private set; }

    public Product                    Product        { get; set; } = null!;
    public Warehouse                  Warehouse      { get; set; } = null!;
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

    protected Stock() { }

    public static Stock Create(Guid productId, Guid warehouseId, int initialQty = 0)
    {
        if (initialQty < 0)
            throw new DomainException("Initial quantity cannot be negative.");

        return new Stock
        {
            ProductId   = productId,
            WarehouseId = warehouseId,
            Quantity    = initialQty
        };
    }

    public StockMovement AddStock(int qty, string movementType,
                                   Guid? referenceId, string? referenceType,
                                   string? notes, Guid? performedBy)
    {
        if (qty <= 0)
            throw new DomainException("Quantity to add must be greater than zero.");

        var before   = Quantity;
        Quantity    += qty;
        UpdatedAt    = DateTime.UtcNow;

        return StockMovement.Create(Id, movementType, qty, before, Quantity,
                                    referenceId, referenceType, notes, performedBy);
    }

    public StockMovement RemoveStock(int qty, string movementType,
                                      Guid? referenceId, string? referenceType,
                                      string? notes, Guid? performedBy)
    {
        if (qty <= 0)
            throw new DomainException("Quantity to remove must be greater than zero.");
        if (qty > Quantity)
            throw new BusinessRuleViolationException(
                $"Insufficient stock. Available: {Quantity}, Requested: {qty}");

        var before   = Quantity;
        Quantity    -= qty;
        UpdatedAt    = DateTime.UtcNow;

        return StockMovement.Create(Id, movementType, -qty, before, Quantity,
                                    referenceId, referenceType, notes, performedBy);
    }

    public void Reserve(int qty)
    {
        if (qty <= 0)
            throw new DomainException("Reserve quantity must be greater than zero.");
        if (qty > AvailableQty)
            throw new BusinessRuleViolationException(
                $"Cannot reserve {qty} units. Available: {AvailableQty}");

        ReservedQty += qty;
        UpdatedAt    = DateTime.UtcNow;
    }

    public void ReleaseReservation(int qty)
    {
        if (qty <= 0)
            throw new DomainException("Release quantity must be greater than zero.");
        if (qty > ReservedQty)
            throw new BusinessRuleViolationException(
                $"Cannot release {qty}. Reserved: {ReservedQty}");

        ReservedQty -= qty;
        UpdatedAt    = DateTime.UtcNow;
    }

    public bool IsLowStock(int reorderLevel)
    {
        return reorderLevel > 0 && AvailableQty <= reorderLevel;
    }

    public void UpdateLastCounted()
    {
        LastCountedAt = DateTime.UtcNow;
        UpdatedAt     = DateTime.UtcNow;
    }
}
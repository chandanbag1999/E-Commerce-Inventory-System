using EcommerceInventory.Domain.Common;
using EcommerceInventory.Domain.Exceptions;

namespace EcommerceInventory.Domain.Entities;

// Entity representing an item in a purchase order
public class PurchaseOrderItem : BaseEntity
{
    public Guid PurchaseOrderId { get; set; }
    public Guid ProductId { get; set; }
    public int QuantityOrdered { get; set; }
    public int QuantityReceived { get; set; } = 0;
    public decimal UnitCost { get; set; }

    // Navigation properties
    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public Product Product { get; set; } = null!;

    // Computed property to get the total cost of this item
    public decimal TotalCost => QuantityOrdered * UnitCost;

    // Factory method to create a new PurchaseOrderItem with validation
    internal static PurchaseOrderItem Create(
        Guid purchaseOrderId,
        Guid productId,
        int quantityOrdered,
        decimal unitCost)
    {
        if (quantityOrdered <= 0)
            throw new DomainException("Quantity must be greater than 0");

        if (unitCost < 0)
            throw new DomainException("Unit cost must be >= 0");

        return new PurchaseOrderItem
        {
            Id = Guid.NewGuid(),
            PurchaseOrderId = purchaseOrderId,
            ProductId = productId,
            QuantityOrdered = quantityOrdered,
            UnitCost = unitCost,
            QuantityReceived = 0,
            CreatedAt = DateTime.UtcNow
        };
    }
}

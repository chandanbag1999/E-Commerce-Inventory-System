using EcommerceInventory.Domain.Common;

namespace EcommerceInventory.Domain.Entities;

public class PurchaseOrderItem : BaseEntity
{
    public Guid    PurchaseOrderId   { get; private set; }
    public Guid    ProductId         { get; private set; }
    public int     QuantityOrdered   { get; private set; }
    public int     QuantityReceived  { get; set; } = 0;
    public decimal UnitCost          { get; private set; }
    public decimal TotalCost         => QuantityOrdered * UnitCost;

    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public Product       Product       { get; set; } = null!;

    protected PurchaseOrderItem() { }

    public static PurchaseOrderItem Create(Guid purchaseOrderId, Guid productId,
                                            int quantityOrdered, decimal unitCost)
    {
        return new PurchaseOrderItem
        {
            PurchaseOrderId  = purchaseOrderId,
            ProductId        = productId,
            QuantityOrdered  = quantityOrdered,
            QuantityReceived = 0,
            UnitCost         = unitCost
        };
    }
}
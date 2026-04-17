using EcommerceInventory.Domain.Common;

namespace EcommerceInventory.Domain.Entities;

public class SalesOrderItem : BaseEntity
{
    public Guid    SalesOrderId { get; private set; }
    public Guid    ProductId    { get; private set; }
    public int     Quantity     { get; private set; }
    public decimal UnitPrice    { get; private set; }
    public decimal Discount     { get; private set; } = 0;
    public decimal LineTotal    => (Quantity * UnitPrice) - Discount;

    public SalesOrder SalesOrder { get; set; } = null!;
    public Product    Product    { get; set; } = null!;

    protected SalesOrderItem() { }

    public static SalesOrderItem Create(Guid salesOrderId, Guid productId,
                                         int quantity, decimal unitPrice,
                                         decimal discount = 0)
    {
        return new SalesOrderItem
        {
            SalesOrderId = salesOrderId,
            ProductId    = productId,
            Quantity     = quantity,
            UnitPrice    = unitPrice,
            Discount     = discount
        };
    }
}
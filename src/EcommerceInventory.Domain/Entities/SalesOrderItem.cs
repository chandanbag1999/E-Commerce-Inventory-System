using EcommerceInventory.Domain.Common;
using EcommerceInventory.Domain.Exceptions;

namespace EcommerceInventory.Domain.Entities;

/// <summary>
/// Sales Order Item entity - line item in a sales order
/// </summary>
public class SalesOrderItem : BaseEntity
{
    public Guid SalesOrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; } = 0;

    // Navigation properties
    public SalesOrder SalesOrder { get; set; } = null!;
    public Product Product { get; set; } = null!;

    /// <summary>
    /// Computed property - total for this line item
    /// </summary>
    public decimal Total => (Quantity * UnitPrice) - Discount;

    /// <summary>
    /// Factory method to create a sales order item
    /// </summary>
    internal static SalesOrderItem Create(
        Guid salesOrderId,
        Guid productId,
        int quantity,
        decimal unitPrice,
        decimal discount = 0)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than 0");

        if (unitPrice < 0)
            throw new DomainException("Unit price must be >= 0");

        if (discount < 0)
            throw new DomainException("Discount must be >= 0");

        return new SalesOrderItem
        {
            Id = Guid.NewGuid(),
            SalesOrderId = salesOrderId,
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Discount = discount,
            CreatedAt = DateTime.UtcNow
        };
    }
}

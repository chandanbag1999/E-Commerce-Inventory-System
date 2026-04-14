using EcommerceInventory.Domain.Common;
using EcommerceInventory.Domain.Enums;
using EcommerceInventory.Domain.Exceptions;

namespace EcommerceInventory.Domain.Entities;

// Sales Order entity representing a customer's order for products
public class SalesOrder : AuditableEntity
{
    public string SoNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = "Walk-in Customer";
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public Guid WarehouseId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Draft;
    public decimal Subtotal { get; set; } = 0;
    public decimal TotalAmount { get; set; } = 0;
    public string? Notes { get; set; }
    public string? ShippingAddressJson { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    // Navigation properties
    public Warehouse Warehouse { get; set; } = null!;
    public ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();

    // Factory method to create a new sales order with validation
    public static SalesOrder Create(
        string soNumber,
        Guid warehouseId,
        Guid createdBy,
        string? customerName = null,
        string? customerEmail = null,
        string? customerPhone = null,
        string? notes = null,
        string? shippingAddressJson = null)
    {
        if (string.IsNullOrWhiteSpace(soNumber))
            throw new DomainException("SO number cannot be empty");

        return new SalesOrder
        {
            Id = Guid.NewGuid(),
            SoNumber = soNumber,
            WarehouseId = warehouseId,
            CustomerName = customerName?.Trim() ?? "Walk-in Customer",
            CustomerEmail = customerEmail?.Trim(),
            CustomerPhone = customerPhone?.Trim(),
            Status = OrderStatus.Draft,
            Subtotal = 0,
            TotalAmount = 0,
            Notes = notes?.Trim(),
            ShippingAddressJson = shippingAddressJson,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// Adds an item to the sales order
    public void AddItem(Guid productId, int quantity, decimal unitPrice, decimal discount = 0)
    {
        if (Status != OrderStatus.Draft)
            throw new BusinessRuleViolationException("Can only add items to Draft orders");

        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than 0");

        if (unitPrice < 0)
            throw new DomainException("Unit price must be >= 0");

        if (discount < 0)
            throw new DomainException("Discount must be >= 0");

        var item = SalesOrderItem.Create(Id, productId, quantity, unitPrice, discount);
        Items.Add(item);
        RecalculateTotal();
    }

    /// Removes an item from the sales order
    public void RemoveItem(Guid itemId)
    {
        if (Status != OrderStatus.Draft)
            throw new BusinessRuleViolationException("Can only remove items from Draft orders");

        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new NotFoundException("Sales order item", itemId);

        Items.Remove(item);
        RecalculateTotal();
    }

    /// Submits the sales order for approval
    public void Submit()
    {
        if (Status != OrderStatus.Draft)
            throw new BusinessRuleViolationException("Only Draft orders can be submitted");

        if (!Items.Any())
            throw new BusinessRuleViolationException("Cannot submit an order with no items");

        Status = OrderStatus.Submitted;
        UpdatedAt = DateTime.UtcNow;
    }

    /// Approves the sales order
    public void Approve(Guid approvedBy)
    {
        if (Status != OrderStatus.Submitted)
            throw new BusinessRuleViolationException("Only Submitted orders can be approved");

        Status = OrderStatus.Approved;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

   /// Ships the sales order
    public void Ship()
    {
        if (Status != OrderStatus.Approved)
            throw new BusinessRuleViolationException("Only Approved orders can be shipped");

        Status = OrderStatus.Shipped;
        ShippedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the order as delivered
    /// </summary>
    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new BusinessRuleViolationException("Only Shipped orders can be delivered");

        Status = OrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancels the sales order
    /// </summary>
    public void Cancel()
    {
        if (Status == OrderStatus.Shipped || Status == OrderStatus.Delivered)
            throw new BusinessRuleViolationException("Cannot cancel shipped or delivered orders");

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Recalculates subtotal and total from all items
    /// </summary>
    private void RecalculateTotal()
    {
        Subtotal = Items.Sum(i => i.Quantity * i.UnitPrice);
        TotalAmount = Items.Sum(i => (i.Quantity * i.UnitPrice) - i.Discount);
        UpdatedAt = DateTime.UtcNow;
    }
}

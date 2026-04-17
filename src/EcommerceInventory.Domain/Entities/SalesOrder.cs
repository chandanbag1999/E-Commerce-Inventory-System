using EcommerceInventory.Domain.Common;
using EcommerceInventory.Domain.Enums;
using EcommerceInventory.Domain.Exceptions;
using EcommerceInventory.Domain.ValueObjects;

namespace EcommerceInventory.Domain.Entities;

public class SalesOrder : BaseEntity
{
    public string   SoNumber          { get; private set; } = string.Empty;
    public string   CustomerName      { get; private set; } = "Walk-in Customer";
    public string?  CustomerEmail     { get; private set; }
    public string?  CustomerPhone     { get; private set; }
    public Guid     WarehouseId       { get; private set; }
    public OrderStatus Status         { get; private set; } = OrderStatus.Draft;
    public decimal  Subtotal          { get; private set; } = 0;
    public decimal  TotalAmount       { get; private set; } = 0;
    public string?  Notes             { get; private set; }
    public Address? ShippingAddress   { get; private set; }
    public Guid     CreatedBy         { get; private set; }
    public Guid?    ApprovedBy        { get; private set; }
    public DateTime? ApprovedAt       { get; private set; }
    public DateTime? ShippedAt        { get; private set; }
    public DateTime? DeliveredAt      { get; private set; }

    public Warehouse                Warehouse { get; set; } = null!;
    public ICollection<SalesOrderItem> Items { get; private set; } = new List<SalesOrderItem>();

    protected SalesOrder() { }

    public static SalesOrder Create(string soNumber, Guid warehouseId, Guid createdBy,
                                     string customerName = "Walk-in Customer",
                                     string? customerEmail = null,
                                     string? customerPhone = null,
                                     string? notes = null,
                                     Address? shippingAddress = null)
    {
        if (string.IsNullOrWhiteSpace(soNumber))
            throw new DomainException("SO Number is required.");

        return new SalesOrder
        {
            SoNumber        = soNumber,
            WarehouseId     = warehouseId,
            CreatedBy       = createdBy,
            CustomerName    = string.IsNullOrWhiteSpace(customerName)
                                ? "Walk-in Customer"
                                : customerName.Trim(),
            CustomerEmail   = customerEmail?.Trim().ToLower(),
            CustomerPhone   = customerPhone?.Trim(),
            Notes           = notes?.Trim(),
            ShippingAddress = shippingAddress,
            Status          = OrderStatus.Draft
        };
    }

    public void AddItem(Guid productId, int quantity, decimal unitPrice, decimal discount = 0)
    {
        if (Status != OrderStatus.Draft)
            throw new BusinessRuleViolationException("Items can only be added to Draft orders.");
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");
        if (unitPrice < 0)
            throw new DomainException("Unit price cannot be negative.");
        if (discount < 0)
            throw new DomainException("Discount cannot be negative.");

        var item = SalesOrderItem.Create(Id, productId, quantity, unitPrice, discount);
        Items.Add(item);
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(Guid itemId)
    {
        if (Status != OrderStatus.Draft)
            throw new BusinessRuleViolationException("Items can only be removed from Draft orders.");

        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new NotFoundException("Sales order item", itemId);

        Items.Remove(item);
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Submit()
    {
        if (Status != OrderStatus.Draft)
            throw new BusinessRuleViolationException("Only Draft orders can be submitted.");
        if (!Items.Any())
            throw new BusinessRuleViolationException("Cannot submit an order with no items.");

        Status    = OrderStatus.Submitted;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve(Guid approvedBy)
    {
        if (Status != OrderStatus.Submitted)
            throw new BusinessRuleViolationException("Only Submitted orders can be approved.");

        Status     = OrderStatus.Approved;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
        UpdatedAt  = DateTime.UtcNow;
    }

    public void Ship()
    {
        if (Status != OrderStatus.Approved)
            throw new BusinessRuleViolationException("Only Approved orders can be shipped.");

        Status    = OrderStatus.Shipped;
        ShippedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new BusinessRuleViolationException("Only Shipped orders can be delivered.");

        Status      = OrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        UpdatedAt   = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Shipped || Status == OrderStatus.Delivered)
            throw new BusinessRuleViolationException("Cannot cancel shipped or delivered orders.");

        Status    = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    private void RecalculateTotal()
    {
        Subtotal    = Items.Sum(i => i.Quantity * i.UnitPrice);
        TotalAmount = Items.Sum(i => (i.Quantity * i.UnitPrice) - i.Discount);
    }
}
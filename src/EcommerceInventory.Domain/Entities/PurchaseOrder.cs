using EcommerceInventory.Domain.Common;
using EcommerceInventory.Domain.Enums;
using EcommerceInventory.Domain.Exceptions;

namespace EcommerceInventory.Domain.Entities;

public class PurchaseOrder : BaseEntity
{
    public string      PoNumber           { get; private set; } = string.Empty;
    public Guid        SupplierId         { get; private set; }
    public Guid        WarehouseId        { get; private set; }
    public OrderStatus Status             { get; private set; } = OrderStatus.Draft;
    public decimal     TotalAmount        { get; private set; } = 0;
    public string?     Notes              { get; private set; }
    public Guid        CreatedBy          { get; private set; }
    public Guid?       ApprovedBy         { get; private set; }
    public DateTime?   ApprovedAt         { get; private set; }
    public DateTime?   ExpectedDeliveryAt { get; private set; }
    public DateTime?   ReceivedAt         { get; private set; }

    public Supplier                   Supplier  { get; set; } = null!;
    public Warehouse                  Warehouse { get; set; } = null!;
    public ICollection<PurchaseOrderItem> Items { get; private set; } = new List<PurchaseOrderItem>();

    protected PurchaseOrder() { }

    public static PurchaseOrder Create(string poNumber, Guid supplierId,
                                        Guid warehouseId, Guid createdBy,
                                        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(poNumber))
            throw new DomainException("PO Number is required.");

        return new PurchaseOrder
        {
            PoNumber    = poNumber,
            SupplierId  = supplierId,
            WarehouseId = warehouseId,
            CreatedBy   = createdBy,
            Notes       = notes?.Trim(),
            Status      = OrderStatus.Draft
        };
    }

    public void SetExpectedDelivery(DateTime? expectedAt)
    {
        ExpectedDeliveryAt = expectedAt;
        UpdatedAt          = DateTime.UtcNow;
    }

    public void AddItem(Guid productId, int quantityOrdered, decimal unitCost)
    {
        if (Status != OrderStatus.Draft)
            throw new BusinessRuleViolationException("Items can only be added to Draft orders.");
        if (quantityOrdered <= 0)
            throw new DomainException("Quantity must be greater than zero.");
        if (unitCost < 0)
            throw new DomainException("Unit cost cannot be negative.");

        var item = PurchaseOrderItem.Create(Id, productId, quantityOrdered, unitCost);
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
            throw new NotFoundException("Purchase order item", itemId);

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

    public void Reject()
    {
        if (Status != OrderStatus.Submitted)
            throw new BusinessRuleViolationException("Only Submitted orders can be rejected.");

        Status    = OrderStatus.Rejected;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkReceived()
    {
        if (Status != OrderStatus.Approved)
            throw new BusinessRuleViolationException("Only Approved orders can be received.");

        Status     = OrderStatus.Received;
        ReceivedAt = DateTime.UtcNow;
        UpdatedAt  = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Received)
            throw new BusinessRuleViolationException("Received orders cannot be cancelled.");

        Status    = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    private void RecalculateTotal()
    {
        TotalAmount = Items.Sum(i => i.QuantityOrdered * i.UnitCost);
    }
}
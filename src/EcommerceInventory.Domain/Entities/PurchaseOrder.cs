using EcommerceInventory.Domain.Common;
using EcommerceInventory.Domain.Enums;
using EcommerceInventory.Domain.Exceptions;

namespace EcommerceInventory.Domain.Entities;

// PurchaseOrder entity representing a purchase order in the system
public class PurchaseOrder : AuditableEntity
{
    public string PoNumber { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public Guid WarehouseId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Draft;
    public decimal TotalAmount { get; set; } = 0;
    public string? Notes { get; set; }
    public DateTime? ExpectedDeliveryAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }

    // Navigation properties
    public Supplier Supplier { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();

    // Factory method to create a new purchase order with validation
    public static PurchaseOrder Create(
        string poNumber,
        Guid supplierId,
        Guid warehouseId,
        Guid createdBy,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(poNumber))
            throw new DomainException("PO number cannot be empty");

        return new PurchaseOrder
        {
            Id = Guid.NewGuid(),
            PoNumber = poNumber,
            SupplierId = supplierId,
            WarehouseId = warehouseId,
            Status = OrderStatus.Draft,
            TotalAmount = 0,
            Notes = notes?.Trim(),
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    // Method to add an item to the purchase order
    public void AddItem(Guid productId, int quantityOrdered, decimal unitCost)
    {
        if (Status != OrderStatus.Draft)
            throw new BusinessRuleViolationException("Can only add items to Draft orders");

        if (quantityOrdered <= 0)
            throw new DomainException("Quantity must be greater than 0");

        if (unitCost < 0)
            throw new DomainException("Unit cost must be >= 0");

        var item = PurchaseOrderItem.Create(Id, productId, quantityOrdered, unitCost);
        Items.Add(item);
        RecalculateTotal();
    }

    // Method to remove an item from the purchase order
    public void RemoveItem(Guid itemId)
    {
        if (Status != OrderStatus.Draft)
            throw new BusinessRuleViolationException("Can only remove items from Draft orders");

        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new NotFoundException("Purchase order item", itemId);

        Items.Remove(item);
        RecalculateTotal();
    }

   // Method to update an item in the purchase order
    public void Submit()
    {
        if (Status != OrderStatus.Draft)
            throw new BusinessRuleViolationException("Only Draft orders can be submitted");

        if (!Items.Any())
            throw new BusinessRuleViolationException("Cannot submit an order with no items");

        Status = OrderStatus.Submitted;
        UpdatedAt = DateTime.UtcNow;
    }

    // Method to approve the purchase order
    public void Approve(Guid approvedBy)
    {
        if (Status != OrderStatus.Submitted)
            throw new BusinessRuleViolationException("Only Submitted orders can be approved");

        Status = OrderStatus.Approved;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Method to reject the purchase order
    public void Reject()
    {
        if (Status != OrderStatus.Submitted)
            throw new BusinessRuleViolationException("Only Submitted orders can be rejected");

        Status = OrderStatus.Rejected;
        UpdatedAt = DateTime.UtcNow;
    }

    // Method to mark the purchase order as received
    public void MarkReceived()
    {
        if (Status != OrderStatus.Approved)
            throw new BusinessRuleViolationException("Only Approved orders can be received");

        Status = OrderStatus.Received;
        ReceivedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Method to cancel the purchase order
    public void Cancel()
    {
        if (Status == OrderStatus.Received)
            throw new BusinessRuleViolationException("Cannot cancel a received order");

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

   // Private method to recalculate the total amount of the purchase order
    private void RecalculateTotal()
    {
        TotalAmount = Items.Sum(i => i.QuantityOrdered * i.UnitCost);
        UpdatedAt = DateTime.UtcNow;
    }
}

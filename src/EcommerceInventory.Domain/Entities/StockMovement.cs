using EcommerceInventory.Domain.Common;

namespace EcommerceInventory.Domain.Entities;

/// <summary>
/// Stock movement entity - audit trail for all stock changes
/// </summary>
public class StockMovement : BaseEntity
{
    public Guid StockId { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int QuantityBefore { get; set; }
    public int QuantityAfter { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public string? Notes { get; set; }
    public Guid? PerformedBy { get; set; }

    // Navigation property
    public Stock Stock { get; set; } = null!;

    /// <summary>
    /// Factory method to create a stock movement record
    /// </summary>
    public static StockMovement Create(
        Guid stockId,
        string movementType,
        int quantity,
        int quantityBefore,
        int quantityAfter,
        Guid? referenceId,
        string? referenceType,
        string? notes,
        Guid? performedBy)
    {
        return new StockMovement
        {
            Id = Guid.NewGuid(),
            StockId = stockId,
            MovementType = movementType,
            Quantity = quantity,
            QuantityBefore = quantityBefore,
            QuantityAfter = quantityAfter,
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            Notes = notes,
            PerformedBy = performedBy,
            CreatedAt = DateTime.UtcNow
        };
    }
}

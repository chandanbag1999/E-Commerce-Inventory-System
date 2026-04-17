using EcommerceInventory.Domain.Common;

namespace EcommerceInventory.Domain.Entities;

public class StockMovement : BaseEntity
{
    public Guid    StockId        { get; private set; }
    public string  MovementType   { get; private set; } = string.Empty;
    public int     Quantity       { get; private set; }
    public int     QuantityBefore { get; private set; }
    public int     QuantityAfter  { get; private set; }
    public Guid?   ReferenceId    { get; private set; }
    public string? ReferenceType  { get; private set; }
    public string? Notes          { get; private set; }
    public Guid?   PerformedBy    { get; private set; }

    public Stock Stock { get; set; } = null!;

    protected StockMovement() { }

    public static StockMovement Create(Guid stockId, string movementType,
                                        int quantity, int quantityBefore, int quantityAfter,
                                        Guid? referenceId, string? referenceType,
                                        string? notes, Guid? performedBy)
    {
        return new StockMovement
        {
            StockId        = stockId,
            MovementType   = movementType,
            Quantity       = quantity,
            QuantityBefore = quantityBefore,
            QuantityAfter  = quantityAfter,
            ReferenceId    = referenceId,
            ReferenceType  = referenceType,
            Notes          = notes,
            PerformedBy    = performedBy
        };
    }
}
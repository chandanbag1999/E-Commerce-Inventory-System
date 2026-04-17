namespace EcommerceInventory.Domain.Enums;

public enum StockMovementType
{
    PurchaseReceived,
    SaleDispatched,
    ManualAdjustmentAdd,
    ManualAdjustmentRemove,
    TransferIn,
    TransferOut,
    StockCount
}
namespace EcommerceInventory.Domain.Enums;

public enum StockMovementType
{
    PurchaseReceived = 0,
    SaleDispatched = 1,
    ManualAdjustmentAdd = 2,
    ManualAdjustmentRemove = 3,
    TransferOut = 4,
    TransferIn = 5,
    Damaged = 6,
    Returned = 7
}

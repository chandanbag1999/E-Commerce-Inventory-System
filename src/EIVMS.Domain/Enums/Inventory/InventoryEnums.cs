namespace EIVMS.Domain.Enums.Inventory;

public enum StockMovementType
{
    StockIn = 1,
    StockOut = 2,
    Reserve = 3,
    Release = 4,
    ManualAdjust = 5,
    Damage = 6,
    WarehouseTransfer = 7,
    CustomerReturn = 8,
    SupplierReturn = 9,
    Opening = 10
}

public enum WarehouseStatus
{
    Active = 1,
    Inactive = 2,
    UnderMaintenance = 3,
    Closed = 4
}

public enum InventoryAlertType
{
    LowStock = 1,
    OutOfStock = 2,
    Overstock = 3,
    ExpiryWarning = 4,
    DamageReport = 5
}

public enum StockReservationStatus
{
    Pending = 1,
    Confirmed = 2,
    Released = 3,
    Expired = 4
}

public enum StockTransferStatus
{
    Pending = 1,
    InTransit = 2,
    Received = 3,
    Cancelled = 4
}
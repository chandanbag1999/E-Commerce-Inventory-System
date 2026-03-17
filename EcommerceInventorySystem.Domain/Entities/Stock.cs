namespace EcommerceInventorySystem.Domain.Entities;

public class Stock : BaseEntity
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public int LowStockThreshold { get; set; } = 10;

    public Product Product { get; set; } = null!;
    public ICollection<StockTransaction> Transactions { get; set; } = new List<StockTransaction>();
}
using EcommerceInventorySystem.Domain.Enums;

namespace EcommerceInventorySystem.Domain.Entities;

public class StockTransaction : BaseEntity
{
    public int ProductId { get; set; }
    public TransactionType TransactionType { get; set; }
    public int Quantity { get; set; }
    public string Note { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;

    public Product Product { get; set; } = null!;
}
namespace EcommerceInventorySystem.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }

    // Foreign keys
    public int CategoryId { get; set; }
    public int SupplierId { get; set; }

    // Navigation properties
    public Category Category { get; set; } = null!;
    public Supplier Supplier { get; set; } = null!;
    public Stock? Stock { get; set; }
}
namespace EcommerceInventorySystem.Domain.Entities;

public class Supplier : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
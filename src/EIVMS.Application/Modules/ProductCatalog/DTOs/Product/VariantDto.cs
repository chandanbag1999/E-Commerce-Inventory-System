namespace EIVMS.Application.Modules.ProductCatalog.DTOs.Product;

public class CreateVariantDto
{
    public string SKU { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Barcode { get; set; }
    public decimal? Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? WeightKg { get; set; }
    public bool IsDefault { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    public int InitialStock { get; set; } = 0;
    public int? LowStockThreshold { get; set; }
    public bool TrackInventory { get; set; } = true;
    public bool AllowBackorder { get; set; } = false;
    public Dictionary<string, string> VariantAttributes { get; set; } = new();
}

public class UpdateVariantDto : CreateVariantDto
{
    public Guid Id { get; set; }
}

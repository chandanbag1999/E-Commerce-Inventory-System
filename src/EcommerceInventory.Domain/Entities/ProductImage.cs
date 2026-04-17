using EcommerceInventory.Domain.Common;

namespace EcommerceInventory.Domain.Entities;

public class ProductImage : BaseEntity
{
    public Guid   ProductId    { get; set; }
    public string CloudinaryId { get; set; } = string.Empty;
    public string ImageUrl     { get; set; } = string.Empty;
    public bool   IsPrimary    { get; set; } = false;
    public int    DisplayOrder { get; set; } = 0;

    public Product Product { get; set; } = null!;

    protected ProductImage() { }

    public static ProductImage Create(Guid productId, string cloudinaryId,
                                       string imageUrl, bool isPrimary = false,
                                       int displayOrder = 0)
    {
        return new ProductImage
        {
            ProductId    = productId,
            CloudinaryId = cloudinaryId,
            ImageUrl     = imageUrl,
            IsPrimary    = isPrimary,
            DisplayOrder = displayOrder
        };
    }
}
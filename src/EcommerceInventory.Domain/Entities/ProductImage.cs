using EcommerceInventory.Domain.Common;
using EcommerceInventory.Domain.Exceptions;

namespace EcommerceInventory.Domain.Entities;

// ProductImage entity representing an image associated with a product
public class ProductImage : BaseEntity
{
    public Guid ProductId { get; set; }
    public string CloudinaryId { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;

    // Navigation property
    public Product Product { get; set; } = null!;

    // Factory method to create a new ProductImage with validation
    public static ProductImage Create(
        Guid productId,
        string cloudinaryId,
        string imageUrl,
        bool isPrimary = false,
        int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(cloudinaryId))
            throw new DomainException("Cloudinary ID cannot be empty");
        
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new DomainException("Image URL cannot be empty");

        return new ProductImage
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            CloudinaryId = cloudinaryId,
            ImageUrl = imageUrl,
            IsPrimary = isPrimary,
            DisplayOrder = displayOrder,
            CreatedAt = DateTime.UtcNow
        };
    }
}

using EIVMS.Domain.Common;

namespace EIVMS.Domain.Entities.ProductCatalog;

public class Tag : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;

    public ICollection<ProductTag> ProductTags { get; private set; } = new List<ProductTag>();

    private Tag() { }

    public static Tag Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tag name is required");

        return new Tag
        {
            Id = Guid.NewGuid(),
            Name = name.Trim().ToLower(),
            Slug = name.Trim().ToLower().Replace(" ", "-"),
            CreatedAt = DateTime.UtcNow
        };
    }
}

public class ProductTag : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Guid TagId { get; private set; }

    public Product Product { get; private set; } = null!;
    public Tag Tag { get; private set; } = null!;

    private ProductTag() { }

    public static ProductTag Create(Guid productId, Guid tagId)
    {
        return new ProductTag
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            TagId = tagId,
            CreatedAt = DateTime.UtcNow
        };
    }
}

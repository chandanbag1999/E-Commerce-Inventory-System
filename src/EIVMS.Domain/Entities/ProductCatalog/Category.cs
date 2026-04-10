using EIVMS.Domain.Common;

namespace EIVMS.Domain.Entities.ProductCatalog;

public class Category : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? ParentId { get; private set; }
    public int DisplayOrder { get; private set; } = 0;
    public string? ImageUrl { get; private set; }
    public string? MetaTitle { get; private set; }
    public string? MetaDescription { get; private set; }
    public string? MetaKeywords { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsDeleted { get; private set; } = false;
    public decimal? CommissionRate { get; private set; }

    public Category? Parent { get; private set; }
    public ICollection<Category> Children { get; private set; } = new List<Category>();
    public ICollection<Product> Products { get; private set; } = new List<Product>();
    public ICollection<AttributeDefinition> AttributeDefinitions { get; private set; } = new List<AttributeDefinition>();

    public static Category Create(
        string name,
        string slug,
        Guid? parentId = null,
        string? description = null,
        int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required");

        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Category slug is required");

        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Slug = slug.ToLowerInvariant().Trim(),
            ParentId = parentId,
            Description = description,
            DisplayOrder = displayOrder,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name,
        string slug,
        string? description,
        int displayOrder,
        decimal? commissionRate)
    {
        Name = name.Trim();
        Slug = slug.ToLowerInvariant().Trim();
        Description = description;
        DisplayOrder = displayOrder;
        CommissionRate = commissionRate;
        SetUpdatedAt();
    }

    public void UpdateSeo(string? metaTitle, string? metaDescription, string? metaKeywords)
    {
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        MetaKeywords = metaKeywords;
        SetUpdatedAt();
    }

    public void SetImage(string imageUrl)
    {
        ImageUrl = imageUrl;
        SetUpdatedAt();
    }

    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        IsActive = false;
        SetUpdatedAt();
    }

    public string GetFullPath()
    {
        return Parent != null ? $"{Parent.GetFullPath()} > {Name}" : Name;
    }
}

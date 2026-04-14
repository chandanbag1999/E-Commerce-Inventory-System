using EcommerceInventory.Domain.Common;
using EcommerceInventory.Domain.Exceptions;

namespace EcommerceInventory.Domain.Entities;

// Category entity - hierarchical product categorization with self-referencing FK
public class Category : AuditableEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? CloudinaryId { get; set; }
    public Guid? ParentId { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
    public ICollection<Product> Products { get; set; } = new List<Product>();

   
    public static Category Create(
        string name,
        string slug,
        string? description = null,
        Guid? parentId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name cannot be empty");
        
        if (string.IsNullOrWhiteSpace(slug))
            throw new DomainException("Category slug cannot be empty");

        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Slug = slug.Trim().ToLower(),
            Description = description?.Trim(),
            ParentId = parentId,
            IsActive = true,
            SortOrder = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

   
    public void SetImage(string imageUrl, string cloudinaryId)
    {
        ImageUrl = imageUrl;
        CloudinaryId = cloudinaryId;
        UpdatedAt = DateTime.UtcNow;
    }

    
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

   
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

   
    public void Delete()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}

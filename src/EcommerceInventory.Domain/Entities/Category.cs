using EcommerceInventory.Domain.Common;
using EcommerceInventory.Domain.Exceptions;

namespace EcommerceInventory.Domain.Entities;

public class Category : BaseEntity, ISoftDelete
{
    public string  Name          { get; private set; } = string.Empty;
    public string  Slug          { get; private set; } = string.Empty;
    public string? Description   { get; private set; }
    public string? ImageUrl      { get; private set; }
    public string? CloudinaryId  { get; private set; }
    public Guid?   ParentId      { get; private set; }
    public bool    IsActive      { get; private set; } = true;
    public int     SortOrder     { get; private set; } = 0;
    public DateTime? DeletedAt   { get; set; }
    public bool    IsDeleted     => DeletedAt.HasValue;

    public Category?              Parent   { get; set; }
    public ICollection<Category>  Children { get; set; } = new List<Category>();
    public ICollection<Product>   Products { get; set; } = new List<Product>();

    protected Category() { }

    public static Category Create(string name, string slug,
                                   string? description = null,
                                   Guid? parentId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name is required.");
        if (string.IsNullOrWhiteSpace(slug))
            throw new DomainException("Slug is required.");

        return new Category
        {
            Name        = name.Trim(),
            Slug        = slug.Trim().ToLower(),
            Description = description?.Trim(),
            ParentId    = parentId,
            IsActive    = true
        };
    }

    public void Update(string name, string? description, Guid? parentId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name is required.");

        Name        = name.Trim();
        Description = description?.Trim();
        ParentId    = parentId;
        UpdatedAt   = DateTime.UtcNow;
    }

    public void SetImage(string imageUrl, string cloudinaryId)
    {
        ImageUrl     = imageUrl;
        CloudinaryId = cloudinaryId;
        UpdatedAt    = DateTime.UtcNow;
    }

    public void SetSortOrder(int order)
    {
        SortOrder = order;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive  = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive  = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
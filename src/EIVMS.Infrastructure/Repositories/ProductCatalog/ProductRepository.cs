using EIVMS.Application.Modules.ProductCatalog.DTOs.Product;
using EIVMS.Application.Modules.ProductCatalog.Interfaces;
using EIVMS.Domain.Entities.ProductCatalog;
using EIVMS.Domain.Enums.ProductCatalog;
using EIVMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EIVMS.Infrastructure.Repositories.ProductCatalog;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<Product?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Products
            .Include(p => p.Category).ThenInclude(c => c!.Parent)
            .Include(p => p.Variants.Where(v => v.IsActive))
            .Include(p => p.Media.Where(m => !m.IsDeleted))
            .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<Product?> GetBySlugAsync(string slug)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Variants.Where(v => v.IsActive))
            .Include(p => p.Media.Where(m => !m.IsDeleted))
            .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Slug == slug.ToLowerInvariant() && !p.IsDeleted);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null)
    {
        var query = _context.Products.Where(p => p.Slug == slug.ToLowerInvariant() && !p.IsDeleted);
        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null)
    {
        var upperSku = sku.ToUpperInvariant().Trim();
        var productSkuExists = await _context.Products
            .Where(p => p.SKU == upperSku && !p.IsDeleted && (excludeId == null || p.Id != excludeId.Value))
            .AnyAsync();
        if (productSkuExists) return true;

        var variantSkuExists = await _context.ProductVariants
            .Where(v => v.SKU == upperSku && (excludeId == null || v.Id != excludeId.Value))
            .AnyAsync();
        return variantSkuExists;
    }

    public async Task<(List<Product> Products, int TotalCount)> GetPagedAsync(ProductFilterDto filter)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Variants.Where(v => v.IsActive))
            .Include(p => p.Media.Where(m => !m.IsDeleted && m.IsPrimary))
            .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
        {
            var search = filter.SearchQuery.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(search) || (p.Brand != null && p.Brand.ToLower().Contains(search)));
        }

        if (filter.CategoryId.HasValue)
        {
            if (filter.IncludeSubCategories)
            {
                var subCategoryIds = await GetSubCategoryIdsAsync(filter.CategoryId.Value);
                subCategoryIds.Add(filter.CategoryId.Value);
                query = query.Where(p => subCategoryIds.Contains(p.CategoryId));
            }
            else
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Brand))
            query = query.Where(p => p.Brand != null && p.Brand.ToLower() == filter.Brand.ToLower());

        if (filter.Status.HasValue)
            query = query.Where(p => p.Status == filter.Status.Value);
        else
            query = query.Where(p => p.Status != ProductStatus.Archived);

        if (filter.Type.HasValue)
            query = query.Where(p => p.Type == filter.Type.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.BasePrice >= filter.MinPrice.Value);
        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.BasePrice <= filter.MaxPrice.Value);

        if (filter.FeaturedOnly == true)
            query = query.Where(p => p.IsFeatured);

        if (filter.VendorId.HasValue)
            query = query.Where(p => p.VendorId == filter.VendorId.Value);

        if (filter.InStockOnly == true)
            query = query.Where(p => p.Variants.Any(v => v.IsActive && v.StockQuantity > v.ReservedQuantity));

        query = (filter.SortBy.ToLower(), filter.SortDirection.ToLower()) switch
        {
            ("name", "asc") => query.OrderBy(p => p.Name),
            ("name", "desc") => query.OrderByDescending(p => p.Name),
            ("price", "asc") => query.OrderBy(p => p.BasePrice),
            ("price", "desc") => query.OrderByDescending(p => p.BasePrice),
            ("createdat", "asc") => query.OrderBy(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        var products = await query.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
        return (products, totalCount);
    }

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task<ProductVariant?> GetVariantByIdAsync(Guid variantId)
    {
        return await _context.ProductVariants.FirstOrDefaultAsync(v => v.Id == variantId);
    }

    public async Task<ProductVariant?> GetVariantBySkuAsync(string sku)
    {
        return await _context.ProductVariants.FirstOrDefaultAsync(v => v.SKU == sku.ToUpperInvariant());
    }

    public async Task AddVariantAsync(ProductVariant variant)
    {
        await _context.ProductVariants.AddAsync(variant);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateVariantAsync(ProductVariant variant)
    {
        _context.ProductVariants.Update(variant);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ProductVariant>> GetVariantsByProductIdAsync(Guid productId)
    {
        return await _context.ProductVariants.Where(v => v.ProductId == productId).OrderBy(v => v.DisplayOrder).ToListAsync();
    }

    public async Task<ProductMedia?> GetMediaByIdAsync(Guid mediaId)
    {
        return await _context.ProductMedias.FirstOrDefaultAsync(m => m.Id == mediaId);
    }

    public async Task AddMediaAsync(ProductMedia media)
    {
        await _context.ProductMedias.AddAsync(media);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateMediaAsync(ProductMedia media)
    {
        _context.ProductMedias.Update(media);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ProductMedia>> GetMediaByProductIdAsync(Guid productId)
    {
        return await _context.ProductMedias.Where(m => m.ProductId == productId).OrderBy(m => m.DisplayOrder).ToListAsync();
    }

    public async Task UnsetAllPrimaryMediaAsync(Guid productId)
    {
        var primaryMedia = await _context.ProductMedias.Where(m => m.ProductId == productId && m.IsPrimary && !m.IsDeleted).ToListAsync();
        foreach (var m in primaryMedia) m.UnsetPrimary();
        await _context.SaveChangesAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid id)
    {
        return await _context.Categories
            .Include(c => c.Parent)
            .Include(c => c.Children.Where(ch => !ch.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task<Category?> GetCategoryByIdIncludingDeletedAsync(Guid id)
    {
        return await _context.Categories
            .Include(c => c.Parent)
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Category?> GetCategoryBySlugAsync(string slug)
    {
        return await _context.Categories.FirstOrDefaultAsync(c => c.Slug == slug.ToLowerInvariant() && !c.IsDeleted);
    }

    public async Task<List<Category>> GetCategoryTreeAsync(Guid? parentId = null)
    {
        return await _context.Categories
            .Include(c => c.Parent)
            .Where(c => !c.IsDeleted && c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }

    public async Task<bool> CategorySlugExistsAsync(string slug, Guid? excludeId = null)
    {
        var query = _context.Categories.Where(c => c.Slug == slug.ToLowerInvariant() && !c.IsDeleted);
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task AddCategoryAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCategoryAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Guid>> GetSubCategoryIdsAsync(Guid categoryId)
    {
        var result = new List<Guid>();
        var queue = new Queue<Guid>();
        queue.Enqueue(categoryId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            var children = await _context.Categories.Where(c => c.ParentId == currentId && !c.IsDeleted).Select(c => c.Id).ToListAsync();
            foreach (var childId in children)
            {
                result.Add(childId);
                queue.Enqueue(childId);
            }
        }
        return result;
    }

    public async Task<List<AttributeDefinition>> GetAttributeDefinitionsAsync(Guid? categoryId = null)
    {
        var query = _context.AttributeDefinitions.Where(a => a.IsActive);
        if (categoryId.HasValue)
            query = query.Where(a => a.CategoryId == null || a.CategoryId == categoryId.Value);
        else
            query = query.Where(a => a.CategoryId == null);
        return await query.OrderBy(a => a.DisplayOrder).ToListAsync();
    }

    public async Task<AttributeDefinition?> GetAttributeDefinitionByIdAsync(Guid id)
    {
        return await _context.AttributeDefinitions.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task AddAttributeDefinitionAsync(AttributeDefinition definition)
    {
        await _context.AttributeDefinitions.AddAsync(definition);
        await _context.SaveChangesAsync();
    }

    public async Task<Tag?> GetOrCreateTagAsync(string name)
    {
        var normalizedName = name.Trim().ToLower();
        var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == normalizedName);
        if (tag == null)
        {
            tag = Tag.Create(normalizedName);
            await _context.Tags.AddAsync(tag);
            await _context.SaveChangesAsync();
        }
        return tag;
    }

    public async Task<List<Tag>> GetAllTagsAsync()
    {
        return await _context.Tags.OrderBy(t => t.Name).ToListAsync();
    }

    public async Task AddProductTagsAsync(List<ProductTag> tags)
    {
        await _context.ProductTags.AddRangeAsync(tags);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveProductTagsAsync(Guid productId)
    {
        var existingTags = await _context.ProductTags.Where(pt => pt.ProductId == productId).ToListAsync();
        _context.ProductTags.RemoveRange(existingTags);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Category>> GetDeletedCategoriesAsync()
    {
        return await _context.Categories
            .Where(c => c.IsDeleted)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync();
    }

    public async Task RestoreCategoryAsync(Guid id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category != null)
        {
            category.Restore();
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> PermanentlyDeleteOldCategoriesAsync(int monthsOld = 12)
    {
        var cutoffDate = DateTime.UtcNow.AddMonths(-monthsOld);
        var oldCategories = await _context.Categories
            .Where(c => c.IsDeleted && c.UpdatedAt < cutoffDate)
            .ToListAsync();
        _context.Categories.RemoveRange(oldCategories);
        await _context.SaveChangesAsync();
        return oldCategories.Count;
    }
}

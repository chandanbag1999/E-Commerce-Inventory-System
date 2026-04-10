using EIVMS.Application.Modules.ProductCatalog.DTOs.Product;
using EIVMS.Domain.Entities.ProductCatalog;

namespace EIVMS.Application.Modules.ProductCatalog.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product?> GetByIdWithDetailsAsync(Guid id);
    Task<Product?> GetBySlugAsync(string slug);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null);
    Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null);
    Task<(List<Product> Products, int TotalCount)> GetPagedAsync(ProductFilterDto filter);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);

    Task<ProductVariant?> GetVariantByIdAsync(Guid variantId);
    Task<ProductVariant?> GetVariantBySkuAsync(string sku);
    Task AddVariantAsync(ProductVariant variant);
    Task UpdateVariantAsync(ProductVariant variant);
    Task<List<ProductVariant>> GetVariantsByProductIdAsync(Guid productId);

    Task<ProductMedia?> GetMediaByIdAsync(Guid mediaId);
    Task AddMediaAsync(ProductMedia media);
    Task UpdateMediaAsync(ProductMedia media);
    Task<List<ProductMedia>> GetMediaByProductIdAsync(Guid productId);
    Task UnsetAllPrimaryMediaAsync(Guid productId);

    Task<Category?> GetCategoryByIdAsync(Guid id);
    Task<Category?> GetCategoryBySlugAsync(string slug);
    Task<List<Category>> GetCategoryTreeAsync(Guid? parentId = null);
    Task<bool> CategorySlugExistsAsync(string slug, Guid? excludeId = null);
    Task AddCategoryAsync(Category category);
    Task UpdateCategoryAsync(Category category);
    Task<List<Guid>> GetSubCategoryIdsAsync(Guid categoryId);

    Task<List<AttributeDefinition>> GetAttributeDefinitionsAsync(Guid? categoryId = null);
    Task<AttributeDefinition?> GetAttributeDefinitionByIdAsync(Guid id);
    Task AddAttributeDefinitionAsync(AttributeDefinition definition);

    Task<Tag?> GetOrCreateTagAsync(string name);
    Task<List<Tag>> GetAllTagsAsync();
    Task AddProductTagsAsync(List<ProductTag> tags);
    Task RemoveProductTagsAsync(Guid productId);
}

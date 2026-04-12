using System.Text.Json;
using EIVMS.Application.Common.Interfaces;
using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.ProductCatalog.DTOs.Category;
using EIVMS.Application.Modules.ProductCatalog.DTOs.Product;
using EIVMS.Application.Modules.ProductCatalog.Interfaces;
using EIVMS.Application.Modules.ProductCatalog.Validators;
using EIVMS.Application.Modules.UserManagement.DTOs.User;
using EIVMS.Domain.Entities.ProductCatalog;
using EIVMS.Domain.Enums.ProductCatalog;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace EIVMS.Application.Modules.ProductCatalog.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IFileStorageService _fileStorage;
    private readonly IValidator<CreateProductDto> _productValidator;
    private readonly IValidator<CreateCategoryDto> _categoryValidator;

    public ProductService(
        IProductRepository repository,
        IFileStorageService fileStorage,
        IValidator<CreateProductDto> productValidator,
        IValidator<CreateCategoryDto> categoryValidator)
    {
        _repository = repository;
        _fileStorage = fileStorage;
        _productValidator = productValidator;
        _categoryValidator = categoryValidator;
    }

    public async Task<ApiResponse<ProductResponseDto>> GetProductByIdAsync(Guid id)
    {
        var product = await _repository.GetByIdWithDetailsAsync(id);
        if (product == null || product.IsDeleted)
            return ApiResponse<ProductResponseDto>.ErrorResponse("Product not found", 404);
        return ApiResponse<ProductResponseDto>.SuccessResponse(MapToDto(product));
    }

    public async Task<ApiResponse<ProductResponseDto>> GetProductBySlugAsync(string slug)
    {
        var product = await _repository.GetBySlugAsync(slug);
        if (product == null || product.IsDeleted)
            return ApiResponse<ProductResponseDto>.ErrorResponse("Product not found", 404);
        return ApiResponse<ProductResponseDto>.SuccessResponse(MapToDto(product));
    }

    public async Task<ApiResponse<PaginatedResponseDto<ProductListResponseDto>>> GetProductsAsync(ProductFilterDto filter)
    {
        if (filter.PageNumber < 1) filter.PageNumber = 1;
        if (filter.PageSize < 1 || filter.PageSize > 100) filter.PageSize = 20;

        var (products, totalCount) = await _repository.GetPagedAsync(filter);
        var dtos = products.Select(MapToListDto).ToList();

        var result = new PaginatedResponseDto<ProductListResponseDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };

        return ApiResponse<PaginatedResponseDto<ProductListResponseDto>>.SuccessResponse(result);
    }

    public async Task<ApiResponse<ProductResponseDto>> CreateProductAsync(CreateProductDto dto, Guid createdByUserId)
    {
        var validationResult = await _productValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return ApiResponse<ProductResponseDto>.ErrorResponse("Validation failed", 422, errors);
        }

        var category = await _repository.GetCategoryByIdAsync(dto.CategoryId);
        if (category == null || !category.IsActive)
            return ApiResponse<ProductResponseDto>.ErrorResponse("Invalid or inactive category", 400);

        var slug = string.IsNullOrWhiteSpace(dto.Slug) ? GenerateSlug(dto.Name) : dto.Slug.ToLowerInvariant().Trim();
        if (await _repository.SlugExistsAsync(slug))
            slug = $"{slug}-{Guid.NewGuid().ToString()[..8]}";

        if (!string.IsNullOrWhiteSpace(dto.SKU) && await _repository.SkuExistsAsync(dto.SKU))
            return ApiResponse<ProductResponseDto>.ErrorResponse($"SKU '{dto.SKU}' already exists", 409);

        var product = Product.Create(dto.Name, slug, dto.CategoryId, dto.BasePrice, createdByUserId, dto.Type, dto.ShortDescription);
        product.UpdateBasicInfo(dto.Name, slug, dto.ShortDescription, dto.FullDescription, dto.Brand, dto.Model, createdByUserId);
        product.UpdatePricing(dto.BasePrice, dto.CompareAtPrice, dto.TaxRate, dto.IsTaxInclusive, dto.HsnCode, dto.PricingType);
        product.UpdatePhysicalAttributes(dto.WeightKg, dto.LengthCm, dto.WidthCm, dto.HeightCm);
        product.UpdateSeo(dto.MetaTitle, dto.MetaDescription, dto.MetaKeywords);

        if (dto.Attributes.Any())
            product.SetAttributesJson(JsonSerializer.Serialize(dto.Attributes));

        await _repository.AddAsync(product);

        if (dto.DefaultVariant != null)
        {
            if (await _repository.SkuExistsAsync(dto.DefaultVariant.SKU))
                return ApiResponse<ProductResponseDto>.ErrorResponse($"Variant SKU '{dto.DefaultVariant.SKU}' already exists", 409);

            var variant = ProductVariant.Create(product.Id, dto.DefaultVariant.SKU, dto.DefaultVariant.Price, dto.DefaultVariant.Name, 0);
            variant.UpdateDetails(dto.DefaultVariant.SKU, dto.DefaultVariant.Name, dto.DefaultVariant.Barcode, dto.DefaultVariant.Price, dto.DefaultVariant.CompareAtPrice, dto.DefaultVariant.CostPrice, dto.DefaultVariant.WeightKg);
            variant.UpdateStock(dto.DefaultVariant.InitialStock);
            variant.SetInventorySettings(dto.DefaultVariant.LowStockThreshold, dto.DefaultVariant.TrackInventory, dto.DefaultVariant.AllowBackorder);

            if (dto.DefaultVariant.VariantAttributes.Any())
                variant.SetVariantAttributes(JsonSerializer.Serialize(dto.DefaultVariant.VariantAttributes));

            variant.SetAsDefault();
            await _repository.AddVariantAsync(variant);
        }

        if (dto.Tags.Any())
        {
            var productTags = new List<ProductTag>();
            foreach (var tagName in dto.Tags.Distinct())
            {
                var tag = await _repository.GetOrCreateTagAsync(tagName);
                productTags.Add(ProductTag.Create(product.Id, tag.Id));
            }
            await _repository.AddProductTagsAsync(productTags);
        }

        var createdProduct = await _repository.GetByIdWithDetailsAsync(product.Id);
        return ApiResponse<ProductResponseDto>.SuccessResponse(MapToDto(createdProduct!), "Product created successfully", 201);
    }

    public async Task<ApiResponse<ProductResponseDto>> UpdateProductAsync(Guid id, UpdateProductDto dto, Guid updatedByUserId)
    {
        var product = await _repository.GetByIdWithDetailsAsync(id);
        if (product == null || product.IsDeleted)
            return ApiResponse<ProductResponseDto>.ErrorResponse("Product not found", 404);

        if (product.Status == ProductStatus.Archived)
            return ApiResponse<ProductResponseDto>.ErrorResponse("Archived products cannot be updated");

        var category = await _repository.GetCategoryByIdAsync(dto.CategoryId);
        if (category == null)
            return ApiResponse<ProductResponseDto>.ErrorResponse("Invalid category", 400);

        var slug = string.IsNullOrWhiteSpace(dto.Slug) ? GenerateSlug(dto.Name) : dto.Slug.ToLowerInvariant().Trim();
        if (await _repository.SlugExistsAsync(slug, id))
            slug = $"{slug}-{Guid.NewGuid().ToString()[..8]}";

        product.UpdateBasicInfo(dto.Name, slug, dto.ShortDescription, dto.FullDescription, dto.Brand, dto.Model, updatedByUserId);
        product.UpdatePricing(dto.BasePrice, dto.CompareAtPrice, dto.TaxRate, dto.IsTaxInclusive, dto.HsnCode, dto.PricingType);
        product.UpdatePhysicalAttributes(dto.WeightKg, dto.LengthCm, dto.WidthCm, dto.HeightCm);
        product.UpdateSeo(dto.MetaTitle, dto.MetaDescription, dto.MetaKeywords);

        if (dto.Attributes.Any())
            product.SetAttributesJson(JsonSerializer.Serialize(dto.Attributes));

        await _repository.UpdateAsync(product);

        await _repository.RemoveProductTagsAsync(id);
        if (dto.Tags.Any())
        {
            var productTags = new List<ProductTag>();
            foreach (var tagName in dto.Tags.Distinct())
            {
                var tag = await _repository.GetOrCreateTagAsync(tagName);
                productTags.Add(ProductTag.Create(id, tag.Id));
            }
            await _repository.AddProductTagsAsync(productTags);
        }

        var updatedProduct = await _repository.GetByIdWithDetailsAsync(id);
        return ApiResponse<ProductResponseDto>.SuccessResponse(MapToDto(updatedProduct!), "Product updated successfully");
    }

    public async Task<ApiResponse<bool>> PublishProductAsync(Guid id, Guid userId)
    {
        var product = await _repository.GetByIdWithDetailsAsync(id);
        if (product == null || product.IsDeleted)
            return ApiResponse<bool>.ErrorResponse("Product not found", 404);

        try
        {
            product.Publish();
            await _repository.UpdateAsync(product);
            return ApiResponse<bool>.SuccessResponse(true, "Product published successfully");
        }
        catch (InvalidOperationException ex)
        {
            return ApiResponse<bool>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> UnpublishProductAsync(Guid id, Guid userId)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null || product.IsDeleted)
            return ApiResponse<bool>.ErrorResponse("Product not found", 404);

        product.Unpublish();
        await _repository.UpdateAsync(product);
        return ApiResponse<bool>.SuccessResponse(true, "Product unpublished");
    }

    public async Task<ApiResponse<bool>> DeleteProductAsync(Guid id, Guid userId)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null || product.IsDeleted)
            return ApiResponse<bool>.ErrorResponse("Product not found", 404);

        product.SoftDelete(userId);
        await _repository.UpdateAsync(product);
        return ApiResponse<bool>.SuccessResponse(true, "Product deleted successfully");
    }

    public async Task<ApiResponse<bool>> SetFeaturedAsync(Guid id, bool isFeatured, Guid userId)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null || product.IsDeleted)
            return ApiResponse<bool>.ErrorResponse("Product not found", 404);

        product.SetFeatured(isFeatured);
        await _repository.UpdateAsync(product);
        return ApiResponse<bool>.SuccessResponse(true, isFeatured ? "Product featured" : "Product unfeatured");
    }

    public async Task<ApiResponse<ProductVariantResponseDto>> AddVariantAsync(Guid productId, CreateVariantDto dto, Guid userId)
    {
        var product = await _repository.GetByIdWithDetailsAsync(productId);
        if (product == null || product.IsDeleted)
            return ApiResponse<ProductVariantResponseDto>.ErrorResponse("Product not found", 404);

        if (await _repository.SkuExistsAsync(dto.SKU))
            return ApiResponse<ProductVariantResponseDto>.ErrorResponse($"SKU '{dto.SKU}' already exists", 409);

        var variant = ProductVariant.Create(productId, dto.SKU, dto.Price, dto.Name, dto.DisplayOrder);
        variant.UpdateDetails(dto.SKU, dto.Name, dto.Barcode, dto.Price, dto.CompareAtPrice, dto.CostPrice, dto.WeightKg);
        variant.UpdateStock(dto.InitialStock);
        variant.SetInventorySettings(dto.LowStockThreshold, dto.TrackInventory, dto.AllowBackorder);

        if (dto.VariantAttributes.Any())
            variant.SetVariantAttributes(JsonSerializer.Serialize(dto.VariantAttributes));

        if (dto.IsDefault || !product.Variants.Any(v => v.IsDefault))
        {
            foreach (var v in product.Variants.Where(v => v.IsDefault))
            {
                v.UnsetDefault();
                await _repository.UpdateVariantAsync(v);
            }
            variant.SetAsDefault();
        }

        await _repository.AddVariantAsync(variant);
        return ApiResponse<ProductVariantResponseDto>.SuccessResponse(MapVariantToDto(variant, product.BasePrice), "Variant added successfully", 201);
    }

    public async Task<ApiResponse<ProductVariantResponseDto>> UpdateVariantAsync(Guid variantId, UpdateVariantDto dto, Guid userId)
    {
        var variant = await _repository.GetVariantByIdAsync(variantId);
        if (variant == null)
            return ApiResponse<ProductVariantResponseDto>.ErrorResponse("Variant not found", 404);

        if (variant.SKU != dto.SKU.ToUpperInvariant().Trim() && await _repository.SkuExistsAsync(dto.SKU, variantId))
            return ApiResponse<ProductVariantResponseDto>.ErrorResponse($"SKU '{dto.SKU}' already exists", 409);

        variant.UpdateDetails(dto.SKU, dto.Name, dto.Barcode, dto.Price, dto.CompareAtPrice, dto.CostPrice, dto.WeightKg);
        variant.SetInventorySettings(dto.LowStockThreshold, dto.TrackInventory, dto.AllowBackorder);

        if (dto.VariantAttributes.Any())
            variant.SetVariantAttributes(JsonSerializer.Serialize(dto.VariantAttributes));

        await _repository.UpdateVariantAsync(variant);
        var product = await _repository.GetByIdAsync(variant.ProductId);
        return ApiResponse<ProductVariantResponseDto>.SuccessResponse(MapVariantToDto(variant, product!.BasePrice), "Variant updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteVariantAsync(Guid variantId, Guid userId)
    {
        var variant = await _repository.GetVariantByIdAsync(variantId);
        if (variant == null)
            return ApiResponse<bool>.ErrorResponse("Variant not found", 404);

        var allVariants = await _repository.GetVariantsByProductIdAsync(variant.ProductId);
        if (allVariants.Count(v => v.IsActive) <= 1)
            return ApiResponse<bool>.ErrorResponse("Cannot delete the last active variant");

        variant.Deactivate();
        await _repository.UpdateVariantAsync(variant);
        return ApiResponse<bool>.SuccessResponse(true, "Variant deleted");
    }

    public async Task<ApiResponse<bool>> SetDefaultVariantAsync(Guid productId, Guid variantId, Guid userId)
    {
        var variants = await _repository.GetVariantsByProductIdAsync(productId);
        var targetVariant = variants.FirstOrDefault(v => v.Id == variantId);
        if (targetVariant == null)
            return ApiResponse<bool>.ErrorResponse("Variant not found", 404);

        foreach (var v in variants.Where(v => v.IsDefault))
        {
            v.UnsetDefault();
            await _repository.UpdateVariantAsync(v);
        }

        targetVariant.SetAsDefault();
        await _repository.UpdateVariantAsync(targetVariant);
        return ApiResponse<bool>.SuccessResponse(true, "Default variant updated");
    }

    public async Task<ApiResponse<ProductMediaDto>> UploadProductImageAsync(Guid productId, IFormFile file, Guid userId)
    {
        var product = await _repository.GetByIdAsync(productId);
        if (product == null || product.IsDeleted)
            return ApiResponse<ProductMediaDto>.ErrorResponse("Product not found", 404);

        if (file == null || file.Length == 0)
            return ApiResponse<ProductMediaDto>.ErrorResponse("No file provided");

        if (file.Length > 10 * 1024 * 1024)
            return ApiResponse<ProductMediaDto>.ErrorResponse("File size cannot exceed 10MB");

        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return ApiResponse<ProductMediaDto>.ErrorResponse("Only JPEG, PNG, and WebP images are allowed");

        var fileName = $"products/{productId}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var url = await _fileStorage.UploadAsync(file, fileName);

        var existingMedia = await _repository.GetMediaByProductIdAsync(productId);
        var activeMedia = existingMedia.Where(m => !m.IsDeleted).ToList();
        var isPrimary = !activeMedia.Any(m => m.IsPrimary);
        var displayOrder = activeMedia.Count;

        var media = ProductMedia.Create(productId, url, MediaType.Image, product.Name, displayOrder, isPrimary);
        media.SetFileInfo(file.FileName, file.Length, file.ContentType);

        await _repository.AddMediaAsync(media);
        return ApiResponse<ProductMediaDto>.SuccessResponse(MapMediaToDto(media), "Image uploaded successfully", 201);
    }

    public async Task<ApiResponse<bool>> DeleteProductMediaAsync(Guid mediaId, Guid userId)
    {
        var media = await _repository.GetMediaByIdAsync(mediaId);
        if (media == null || media.IsDeleted)
            return ApiResponse<bool>.ErrorResponse("Media not found", 404);

        var wasPrimary = media.IsPrimary;
        media.SoftDelete();
        await _repository.UpdateMediaAsync(media);

        if (wasPrimary)
        {
            var remaining = await _repository.GetMediaByProductIdAsync(media.ProductId);
            var firstActive = remaining.FirstOrDefault(m => !m.IsDeleted);
            if (firstActive != null)
            {
                firstActive.SetAsPrimary();
                await _repository.UpdateMediaAsync(firstActive);
            }
        }

        return ApiResponse<bool>.SuccessResponse(true, "Media deleted");
    }

    public async Task<ApiResponse<bool>> SetPrimaryImageAsync(Guid productId, Guid mediaId, Guid userId)
    {
        var media = await _repository.GetMediaByIdAsync(mediaId);
        if (media == null || media.IsDeleted || media.ProductId != productId)
            return ApiResponse<bool>.ErrorResponse("Media not found", 404);

        await _repository.UnsetAllPrimaryMediaAsync(productId);
        media.SetAsPrimary();
        await _repository.UpdateMediaAsync(media);
        return ApiResponse<bool>.SuccessResponse(true, "Primary image updated");
    }

    public async Task<ApiResponse<bool>> UpdateMediaOrderAsync(Guid productId, List<Guid> orderedMediaIds, Guid userId)
    {
        var allMedia = await _repository.GetMediaByProductIdAsync(productId);
        for (int i = 0; i < orderedMediaIds.Count; i++)
        {
            var media = allMedia.FirstOrDefault(m => m.Id == orderedMediaIds[i]);
            if (media != null)
            {
                media.UpdateDisplayOrder(i);
                await _repository.UpdateMediaAsync(media);
            }
        }
        return ApiResponse<bool>.SuccessResponse(true, "Media order updated");
    }

    public async Task<ApiResponse<CategoryResponseDto>> GetCategoryByIdAsync(Guid id, bool includeDeleted = false)
    {
        var category = await _repository.GetCategoryByIdAsync(id);
        if (category == null)
            return ApiResponse<CategoryResponseDto>.ErrorResponse("Category not found", 404);
        if (!includeDeleted && category.IsDeleted)
            return ApiResponse<CategoryResponseDto>.ErrorResponse("Category not found", 404);
        return ApiResponse<CategoryResponseDto>.SuccessResponse(MapCategoryToDto(category));
    }

    public async Task<ApiResponse<List<CategoryResponseDto>>> GetCategoryTreeAsync()
    {
        var allCategories = await _repository.GetCategoryTreeAsync();
        var tree = BuildCategoryTree(allCategories, null);
        return ApiResponse<List<CategoryResponseDto>>.SuccessResponse(tree);
    }

    public async Task<ApiResponse<CategoryResponseDto>> CreateCategoryAsync(CreateCategoryDto dto)
    {
        var validationResult = await _categoryValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return ApiResponse<CategoryResponseDto>.ErrorResponse("Validation failed", 422, errors);
        }

        var slug = string.IsNullOrWhiteSpace(dto.Slug) ? GenerateSlug(dto.Name) : dto.Slug.ToLowerInvariant().Trim();
        if (await _repository.CategorySlugExistsAsync(slug))
            return ApiResponse<CategoryResponseDto>.ErrorResponse($"Category slug '{slug}' already exists", 409);

        if (dto.ParentId.HasValue)
        {
            var parent = await _repository.GetCategoryByIdAsync(dto.ParentId.Value);
            if (parent == null || parent.IsDeleted)
                return ApiResponse<CategoryResponseDto>.ErrorResponse("Parent category not found", 400);
        }

        var category = Category.Create(dto.Name, slug, dto.ParentId, dto.Description, dto.DisplayOrder);
        category.UpdateSeo(dto.MetaTitle, dto.MetaDescription, dto.MetaKeywords);
        category.Update(dto.Name, slug, dto.Description, dto.DisplayOrder, dto.CommissionRate);

        await _repository.AddCategoryAsync(category);
        return ApiResponse<CategoryResponseDto>.SuccessResponse(MapCategoryToDto(category), "Category created successfully", 201);
    }

    public async Task<ApiResponse<CategoryResponseDto>> UpdateCategoryAsync(Guid id, CreateCategoryDto dto)
    {
        var category = await _repository.GetCategoryByIdAsync(id);
        if (category == null || category.IsDeleted)
            return ApiResponse<CategoryResponseDto>.ErrorResponse("Category not found", 404);

        var slug = string.IsNullOrWhiteSpace(dto.Slug) ? GenerateSlug(dto.Name) : dto.Slug.ToLowerInvariant().Trim();
        if (await _repository.CategorySlugExistsAsync(slug, id))
            return ApiResponse<CategoryResponseDto>.ErrorResponse($"Slug '{slug}' already exists", 409);

        category.Update(dto.Name, slug, dto.Description, dto.DisplayOrder, dto.CommissionRate);
        category.UpdateSeo(dto.MetaTitle, dto.MetaDescription, dto.MetaKeywords);

        await _repository.UpdateCategoryAsync(category);
        return ApiResponse<CategoryResponseDto>.SuccessResponse(MapCategoryToDto(category), "Category updated");
    }

    public async Task<ApiResponse<bool>> DeleteCategoryAsync(Guid id)
    {
        var category = await _repository.GetCategoryByIdAsync(id);
        if (category == null || category.IsDeleted)
            return ApiResponse<bool>.ErrorResponse("Category not found", 404);

        if (category.Products.Any(p => !p.IsDeleted))
            return ApiResponse<bool>.ErrorResponse("Cannot delete category with active products");

        if (category.Children.Any(c => !c.IsDeleted))
            return ApiResponse<bool>.ErrorResponse("Cannot delete category with subcategories");

        category.SoftDelete();
        await _repository.UpdateCategoryAsync(category);
        return ApiResponse<bool>.SuccessResponse(true, "Category deleted");
    }

    public async Task<ApiResponse<string>> UploadCategoryImageAsync(Guid id, IFormFile file)
    {
        var category = await _repository.GetCategoryByIdAsync(id);
        if (category == null)
            return ApiResponse<string>.ErrorResponse("Category not found", 404);

        if (file == null || file.Length == 0)
            return ApiResponse<string>.ErrorResponse("No file provided");

        var fileName = $"categories/{id}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var url = await _fileStorage.UploadAsync(file, fileName);

        category.SetImage(url);
        await _repository.UpdateCategoryAsync(category);
        return ApiResponse<string>.SuccessResponse(url, "Category image uploaded");
    }

    public async Task<ApiResponse<List<CategoryResponseDto>>> GetDeletedCategoriesAsync()
    {
        var categories = await _repository.GetDeletedCategoriesAsync();
        var dtos = categories.Select(MapCategoryToDto).ToList();
        return ApiResponse<List<CategoryResponseDto>>.SuccessResponse(dtos, "Deleted categories retrieved");
    }

    public async Task<ApiResponse<bool>> RestoreCategoryAsync(Guid id)
    {
        try
        {
            // Use GetCategoryByIdIncludingDeleted to find deleted categories
            var category = await _repository.GetCategoryByIdIncludingDeletedAsync(id);
            
            if (category == null)
                return ApiResponse<bool>.ErrorResponse("Category not found", 404);
            
            if (!category.IsDeleted)
                return ApiResponse<bool>.ErrorResponse("Category is not deleted", 400);

            await _repository.RestoreCategoryAsync(id);
            return ApiResponse<bool>.SuccessResponse(true, "Category restored successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse($"Failed to restore category: {ex.Message}", 500);
        }
    }

    public async Task<ApiResponse<int>> PermanentlyDeleteOldCategoriesAsync(int monthsOld = 12)
    {
        var deletedCount = await _repository.PermanentlyDeleteOldCategoriesAsync(monthsOld);
        return ApiResponse<int>.SuccessResponse(deletedCount, $"{deletedCount} old categories permanently deleted");
    }

    private static string GenerateSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var slug = input.ToLowerInvariant().Trim();
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
        return slug.Trim('-');
    }

    private static List<CategoryResponseDto> BuildCategoryTree(List<Category> allCategories, Guid? parentId)
    {
        return allCategories
            .Where(c => c.ParentId == parentId && !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .Select(c =>
            {
                var dto = MapCategoryToDto(c);
                dto.Children = BuildCategoryTree(allCategories, c.Id);
                return dto;
            })
            .ToList();
    }

    private static ProductResponseDto MapToDto(Product product)
    {
        var primaryMedia = product.Media.Where(m => !m.IsDeleted).OrderByDescending(m => m.IsPrimary).ThenBy(m => m.DisplayOrder).FirstOrDefault();
        var attributes = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(product.AttributesJson))
        {
            try { attributes = JsonSerializer.Deserialize<Dictionary<string, string>>(product.AttributesJson) ?? new(); } catch { }
        }

        var variants = product.Variants.Where(v => v.IsActive).OrderBy(v => v.DisplayOrder).ToList();

        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            ShortDescription = product.ShortDescription,
            FullDescription = product.FullDescription,
            SKU = product.SKU,
            Brand = product.Brand,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? "",
            Type = product.Type,
            Status = product.Status,
            BasePrice = product.BasePrice,
            CompareAtPrice = product.CompareAtPrice,
            DiscountPercentage = product.DiscountPercentage,
            PriceWithTax = product.PriceWithTax,
            Currency = product.Currency,
            TaxRate = product.TaxRate,
            IsTaxInclusive = product.IsTaxInclusive,
            WeightKg = product.WeightKg,
            MetaTitle = product.MetaTitle,
            MetaDescription = product.MetaDescription,
            PrimaryImageUrl = primaryMedia?.Url,
            Media = product.Media.Where(m => !m.IsDeleted).OrderBy(m => m.DisplayOrder).Select(MapMediaToDto).ToList(),
            Variants = variants.Select(v => MapVariantToDto(v, product.BasePrice)).ToList(),
            TotalVariants = variants.Count,
            TotalStock = variants.Sum(v => v.AvailableStock),
            Attributes = attributes,
            Tags = product.ProductTags.Select(pt => pt.Tag?.Name ?? "").Where(t => !string.IsNullOrEmpty(t)).ToList(),
            IsFeatured = product.IsFeatured,
            IsPublished = product.IsPublished,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            PublishedAt = product.PublishedAt
        };
    }

    private static ProductListResponseDto MapToListDto(Product product)
    {
        var primaryMedia = product.Media.FirstOrDefault(m => !m.IsDeleted && m.IsPrimary);
        var totalStock = product.Variants.Where(v => v.IsActive).Sum(v => v.AvailableStock);

        return new ProductListResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            Brand = product.Brand,
            BasePrice = product.BasePrice,
            CompareAtPrice = product.CompareAtPrice,
            DiscountPercentage = product.DiscountPercentage,
            Status = product.Status,
            PrimaryImageUrl = primaryMedia?.Url,
            CategoryName = product.Category?.Name ?? "",
            TotalStock = totalStock,
            IsFeatured = product.IsFeatured,
            CreatedAt = product.CreatedAt
        };
    }

    private static ProductVariantResponseDto MapVariantToDto(ProductVariant variant, decimal productBasePrice)
    {
        var variantAttrs = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(variant.VariantAttributesJson))
        {
            try { variantAttrs = JsonSerializer.Deserialize<Dictionary<string, string>>(variant.VariantAttributesJson) ?? new(); } catch { }
        }

        return new ProductVariantResponseDto
        {
            Id = variant.Id,
            SKU = variant.SKU,
            Name = variant.Name,
            Barcode = variant.Barcode,
            Price = variant.Price,
            CompareAtPrice = variant.CompareAtPrice,
            CostPrice = variant.CostPrice,
            StockQuantity = variant.StockQuantity,
            AvailableStock = variant.AvailableStock,
            IsInStock = variant.IsInStock,
            IsLowStock = variant.IsLowStock,
            IsDefault = variant.IsDefault,
            IsActive = variant.IsActive,
            ImageUrl = variant.ImageUrl,
            VariantAttributes = variantAttrs,
            DisplayOrder = variant.DisplayOrder
        };
    }

    private static ProductMediaDto MapMediaToDto(ProductMedia media)
    {
        return new ProductMediaDto
        {
            Id = media.Id,
            Url = media.Url,
            ThumbnailUrl = media.ThumbnailUrl,
            Type = media.Type,
            AltText = media.AltText,
            IsPrimary = media.IsPrimary,
            DisplayOrder = media.DisplayOrder,
            MimeType = media.MimeType
        };
    }

    private static CategoryResponseDto MapCategoryToDto(Category category)
    {
        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            ParentId = category.ParentId,
            ParentName = category.Parent?.Name,
            FullPath = category.GetFullPath(),
            DisplayOrder = category.DisplayOrder,
            ImageUrl = category.ImageUrl,
            IsActive = category.IsActive,
            IsDeleted = category.IsDeleted,
            DeletedAt = category.IsDeleted ? category.UpdatedAt : null,
            ProductCount = category.Products.Count(p => !p.IsDeleted),
            CommissionRate = category.CommissionRate,
            MetaTitle = category.MetaTitle,
            MetaDescription = category.MetaDescription,
            Children = new List<CategoryResponseDto>()
        };
    }
}

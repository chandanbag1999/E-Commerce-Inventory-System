using EIVMS.Application.Common.Interfaces;
using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.ProductCatalog.DTOs.Category;
using EIVMS.Application.Modules.ProductCatalog.DTOs.Product;
using EIVMS.Application.Modules.UserManagement.DTOs.User;
using Microsoft.AspNetCore.Http;

namespace EIVMS.Application.Modules.ProductCatalog.Interfaces;

public interface IProductService
{
    Task<ApiResponse<ProductResponseDto>> GetProductByIdAsync(Guid id);
    Task<ApiResponse<ProductResponseDto>> GetProductBySlugAsync(string slug);
    Task<ApiResponse<PaginatedResponseDto<ProductListResponseDto>>> GetProductsAsync(ProductFilterDto filter);
    Task<ApiResponse<ProductResponseDto>> CreateProductAsync(CreateProductDto dto, Guid createdByUserId);
    Task<ApiResponse<ProductResponseDto>> UpdateProductAsync(Guid id, UpdateProductDto dto, Guid updatedByUserId);
    Task<ApiResponse<bool>> PublishProductAsync(Guid id, Guid userId);
    Task<ApiResponse<bool>> UnpublishProductAsync(Guid id, Guid userId);
    Task<ApiResponse<bool>> DeleteProductAsync(Guid id, Guid userId);
    Task<ApiResponse<bool>> SetFeaturedAsync(Guid id, bool isFeatured, Guid userId);

    Task<ApiResponse<ProductVariantResponseDto>> AddVariantAsync(Guid productId, CreateVariantDto dto, Guid userId);
    Task<ApiResponse<ProductVariantResponseDto>> UpdateVariantAsync(Guid variantId, UpdateVariantDto dto, Guid userId);
    Task<ApiResponse<bool>> DeleteVariantAsync(Guid variantId, Guid userId);
    Task<ApiResponse<bool>> SetDefaultVariantAsync(Guid productId, Guid variantId, Guid userId);

    Task<ApiResponse<ProductMediaDto>> UploadProductImageAsync(Guid productId, IFormFile file, Guid userId);
    Task<ApiResponse<bool>> DeleteProductMediaAsync(Guid mediaId, Guid userId);
    Task<ApiResponse<bool>> SetPrimaryImageAsync(Guid productId, Guid mediaId, Guid userId);
    Task<ApiResponse<bool>> UpdateMediaOrderAsync(Guid productId, List<Guid> orderedMediaIds, Guid userId);

    Task<ApiResponse<CategoryResponseDto>> GetCategoryByIdAsync(Guid id);
    Task<ApiResponse<List<CategoryResponseDto>>> GetCategoryTreeAsync();
    Task<ApiResponse<CategoryResponseDto>> CreateCategoryAsync(CreateCategoryDto dto);
    Task<ApiResponse<CategoryResponseDto>> UpdateCategoryAsync(Guid id, CreateCategoryDto dto);
    Task<ApiResponse<bool>> DeleteCategoryAsync(Guid id);
    Task<ApiResponse<string>> UploadCategoryImageAsync(Guid id, IFormFile file);
}

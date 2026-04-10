using EIVMS.Application.Modules.Orders.Interfaces;
using EIVMS.Application.Modules.ProductCatalog.Interfaces;
using Microsoft.Extensions.Logging;

namespace EIVMS.Application.Modules.Orders.Services.Integration;

public class ProductIntegrationService : IProductIntegrationService
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductIntegrationService> _logger;

    public ProductIntegrationService(IProductService productService, ILogger<ProductIntegrationService> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    public async Task<ProductDetailsForOrderDto?> GetProductDetailsAsync(Guid productId, Guid? variantId = null)
    {
        try
        {
            var response = await _productService.GetProductByIdAsync(productId);
            if (response.Data == null)
            {
                _logger.LogWarning("Product not found: {ProductId}", productId);
                return null;
            }

            var product = response.Data;
            var variant = variantId.HasValue && product.Variants != null 
                ? product.Variants.FirstOrDefault(v => v.Id == variantId) 
                : null;

            var basePrice = variant?.Price ?? product.BasePrice;
            var taxRate = product.TaxRate > 0 ? product.TaxRate : 0;

            return new ProductDetailsForOrderDto
            {
                ProductId = product.Id,
                VariantId = variant?.Id,
                SKU = !string.IsNullOrEmpty(variant?.SKU) ? variant.SKU : product.SKU ?? string.Empty,
                ProductName = product.Name,
                VariantName = variant?.Name,
                ImageUrl = variant?.ImageUrl ?? product.PrimaryImageUrl,
                BasePrice = basePrice,
                TaxRate = taxRate,
                IsAvailable = product.IsPublished,
                VendorId = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching product details for {ProductId}", productId);
            return null;
        }
    }

    public async Task<List<ProductDetailsForOrderDto>> GetMultipleProductDetailsAsync(List<(Guid ProductId, Guid? VariantId)> products)
    {
        var results = new List<ProductDetailsForOrderDto>();

        foreach (var (productId, variantId) in products)
        {
            var details = await GetProductDetailsAsync(productId, variantId);
            if (details != null)
            {
                results.Add(details);
            }
        }

        return results;
    }
}
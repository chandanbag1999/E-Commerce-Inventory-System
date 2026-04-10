using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using EIVMS.Application.Modules.ProductCatalog.Interfaces;
using EIVMS.Application.Modules.ProductCatalog.DTOs.Product;

namespace EIVMS.API.Controllers.v1.ProductCatalog;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProductsController(IProductService productService, IHttpContextAccessor httpContextAccessor)
    {
        _productService = productService;
        _httpContextAccessor = httpContextAccessor;
    }

    private Guid GetUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value 
            ?? _httpContextAccessor.HttpContext?.User.FindFirst("userId")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetProducts([FromQuery] ProductFilterDto filter)
    {
        var result = await _productService.GetProductsAsync(filter);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var result = await _productService.GetProductByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductBySlug(string slug)
    {
        var result = await _productService.GetProductBySlugAsync(slug);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Policy = "product:create")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized(new { success = false, message = "Invalid user" });

        var result = await _productService.CreateProductAsync(dto, userId);
        return result.Success ? CreatedAtAction(nameof(GetProduct), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "product:update")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized(new { success = false, message = "Invalid user" });

        var result = await _productService.UpdateProductAsync(id, dto, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/publish")]
    [Authorize(Policy = "product:update")]
    public async Task<IActionResult> PublishProduct(Guid id)
    {
        var userId = GetUserId();
        var result = await _productService.PublishProductAsync(id, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/unpublish")]
    [Authorize(Policy = "product:update")]
    public async Task<IActionResult> UnpublishProduct(Guid id)
    {
        var userId = GetUserId();
        var result = await _productService.UnpublishProductAsync(id, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "product:delete")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var userId = GetUserId();
        var result = await _productService.DeleteProductAsync(id, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/feature")]
    [Authorize(Policy = "product:update")]
    public async Task<IActionResult> SetFeatured(Guid id, [FromQuery] bool isFeatured = true)
    {
        var userId = GetUserId();
        var result = await _productService.SetFeaturedAsync(id, isFeatured, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{productId:guid}/variants")]
    [Authorize(Policy = "product:update")]
    public async Task<IActionResult> AddVariant(Guid productId, [FromBody] CreateVariantDto dto)
    {
        var userId = GetUserId();
        var result = await _productService.AddVariantAsync(productId, dto, userId);
        return result.Success ? CreatedAtAction(nameof(GetProduct), new { id = productId }, result) : BadRequest(result);
    }

    [HttpPut("variants/{variantId:guid}")]
    [Authorize(Policy = "product:update")]
    public async Task<IActionResult> UpdateVariant(Guid variantId, [FromBody] UpdateVariantDto dto)
    {
        var userId = GetUserId();
        var result = await _productService.UpdateVariantAsync(variantId, dto, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("variants/{variantId:guid}")]
    [Authorize(Policy = "product:update")]
    public async Task<IActionResult> DeleteVariant(Guid variantId)
    {
        var userId = GetUserId();
        var result = await _productService.DeleteVariantAsync(variantId, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{productId:guid}/variants/{variantId:guid}/default")]
    [Authorize(Policy = "product:update")]
    public async Task<IActionResult> SetDefaultVariant(Guid productId, Guid variantId)
    {
        var userId = GetUserId();
        var result = await _productService.SetDefaultVariantAsync(productId, variantId, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{productId:guid}/images")]
    [Authorize(Policy = "product:update")]
    public async Task<IActionResult> UploadImage(Guid productId, IFormFile file)
    {
        var userId = GetUserId();
        var result = await _productService.UploadProductImageAsync(productId, file, userId);
        return result.Success ? CreatedAtAction(nameof(GetProduct), new { id = productId }, result) : BadRequest(result);
    }

    [HttpDelete("images/{mediaId:guid}")]
    [Authorize(Policy = "product:update")]
    public async Task<IActionResult> DeleteImage(Guid mediaId)
    {
        var userId = GetUserId();
        var result = await _productService.DeleteProductMediaAsync(mediaId, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{productId:guid}/images/{mediaId:guid}/primary")]
    [Authorize(Policy = "product:update")]
    public async Task<IActionResult> SetPrimaryImage(Guid productId, Guid mediaId)
    {
        var userId = GetUserId();
        var result = await _productService.SetPrimaryImageAsync(productId, mediaId, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{productId:guid}/images/order")]
    [Authorize(Policy = "product:update")]
    public async Task<IActionResult> UpdateImageOrder(Guid productId, [FromBody] List<Guid> orderedMediaIds)
    {
        var userId = GetUserId();
        var result = await _productService.UpdateMediaOrderAsync(productId, orderedMediaIds, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
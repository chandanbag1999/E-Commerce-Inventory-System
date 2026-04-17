using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Products.Commands.CreateProduct;
using EcommerceInventory.Application.Features.Products.Commands.DeleteProduct;
using EcommerceInventory.Application.Features.Products.Commands.DeleteProductImage;
using EcommerceInventory.Application.Features.Products.Commands.UpdateProduct;
using EcommerceInventory.Application.Features.Products.Commands.UploadProductImages;
using EcommerceInventory.Application.Features.Products.Queries.GetAllProducts;
using EcommerceInventory.Application.Features.Products.Queries.GetProductById;
using EcommerceInventory.Application.Features.Products.Queries.GetProductBySku;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceInventory.API.Controllers;

[Authorize]
public class ProductsController : BaseApiController
{
    private readonly ICurrentUserService _currentUser;

    public ProductsController(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    // GET /api/v1/products
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetAllProductsQuery query,
        CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    // GET /api/v1/products/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new GetProductByIdQuery { Id = id }, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    // GET /api/v1/products/sku/{sku}
    [HttpGet("sku/{sku}")]
    public async Task<IActionResult> GetBySku(string sku, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new GetProductBySkuQuery { Sku = sku }, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    // POST /api/v1/products
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] CreateProductRequestDto request,
        CancellationToken ct)
    {
        var imageUploads = new List<ProductImageUpload>();

        if (request.Images != null)
        {
            foreach (var file in request.Images)
            {
                if (file.Length > 0)
                {
                    imageUploads.Add(new ProductImageUpload
                    {
                        FileStream  = file.OpenReadStream(),
                        FileName    = file.FileName,
                        ContentType = file.ContentType
                    });
                }
            }
        }

        var command = new CreateProductCommand
        {
            CategoryId   = request.CategoryId,
            Name         = request.Name,
            Sku          = request.Sku,
            Description  = request.Description,
            UnitPrice    = request.UnitPrice,
            CostPrice    = request.CostPrice,
            ReorderLevel = request.ReorderLevel,
            ReorderQty   = request.ReorderQty,
            Barcode      = request.Barcode,
            WeightKg     = request.WeightKg,
            CreatedBy    = _currentUser.UserId,
            Images       = imageUploads
        };

        var result = await Mediator.Send(command, ct);
        return StatusCode(201, ApiResponse<object>.Ok(result,
            "Product created successfully."));
    }

    // PUT /api/v1/products/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProductCommand command,
        CancellationToken ct)
    {
        command.Id = id;
        var result = await Mediator.Send(command, ct);
        return Ok(ApiResponse<object>.Ok(result,
            "Product updated successfully."));
    }

    // DELETE /api/v1/products/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteProductCommand { Id = id }, ct);
        return Ok(ApiResponse.Ok("Product deleted successfully."));
    }

    // POST /api/v1/products/{id}/images
    [HttpPost("{id:guid}/images")]
    public async Task<IActionResult> UploadImages(
        Guid id, List<IFormFile> files, CancellationToken ct)
    {
        if (files == null || !files.Any())
            return BadRequest(ApiResponse.Fail("At least one file is required."));

        var uploads = files
            .Where(f => f.Length > 0)
            .Select(f => new ProductFileUpload
            {
                FileStream  = f.OpenReadStream(),
                FileName    = f.FileName,
                ContentType = f.ContentType
            }).ToList();

        var command = new UploadProductImagesCommand
        {
            ProductId = id,
            Files     = uploads
        };

        var imageUrls = await Mediator.Send(command, ct);
        return Ok(ApiResponse<object>.Ok(
            new { imageUrls }, "Images uploaded successfully."));
    }

    // DELETE /api/v1/products/{id}/images/{imageId}
    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> DeleteImage(
        Guid id, Guid imageId, CancellationToken ct)
    {
        await Mediator.Send(new DeleteProductImageCommand
        {
            ProductId = id,
            ImageId   = imageId
        }, ct);
        return Ok(ApiResponse.Ok("Image deleted successfully."));
    }
}

public class CreateProductRequestDto
{
    public Guid    CategoryId   { get; set; }
    public string  Name         { get; set; } = string.Empty;
    public string  Sku          { get; set; } = string.Empty;
    public string? Description  { get; set; }
    public decimal UnitPrice    { get; set; }
    public decimal CostPrice    { get; set; }
    public int     ReorderLevel { get; set; } = 0;
    public int     ReorderQty   { get; set; } = 0;
    public string? Barcode      { get; set; }
    public decimal? WeightKg    { get; set; }
    public List<IFormFile>? Images { get; set; }
}

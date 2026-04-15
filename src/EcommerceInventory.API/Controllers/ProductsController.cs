using EcommerceInventory.API.Authorization;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Products.Commands.CreateProduct;
using EcommerceInventory.Application.Features.Products.Commands.DeleteProduct;
using EcommerceInventory.Application.Features.Products.Commands.DeleteProductImage;
using EcommerceInventory.Application.Features.Products.Commands.UpdateProduct;
using EcommerceInventory.Application.Features.Products.Commands.UploadProductImages;
using EcommerceInventory.Application.Features.Products.DTOs;
using EcommerceInventory.Application.Features.Products.Queries.GetAllProducts;
using EcommerceInventory.Application.Features.Products.Queries.GetProductById;
using EcommerceInventory.Application.Features.Products.Queries.GetProductBySku;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceInventory.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission("Products.View")]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? status = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] string sortBy = "Name",
        [FromQuery] bool sortDesc = false,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAllProductsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            CategoryId = categoryId,
            Status = status,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            SortBy = sortBy,
            SortDesc = sortDesc
        }, ct);
        return Ok(new ApiResponse<PagedResult<ProductListDto>>(true, result.Data!));
    }

    [HasPermission("Products.View")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id), ct);
        return result.Success ? Ok(new ApiResponse<ProductDto>(true, result.Data!)) : NotFound(new ApiResponse<object>(false, result.Message!));
    }

    [HasPermission("Products.View")]
    [HttpGet("sku/{sku}")]
    public async Task<IActionResult> GetBySku(string sku, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductBySkuQuery(sku), ct);
        return result.Success ? Ok(new ApiResponse<ProductDto>(true, result.Data!)) : NotFound(new ApiResponse<object>(false, result.Message!));
    }

    [HasPermission("Products.Create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateProductDto dto, List<IFormFile>? images = null, CancellationToken ct = default)
    {
        var command = new CreateProductCommand
        {
            CategoryId = dto.CategoryId,
            Name = dto.Name,
            Sku = dto.Sku,
            Description = dto.Description,
            UnitPrice = dto.UnitPrice,
            CostPrice = dto.CostPrice,
            ReorderLevel = dto.ReorderLevel,
            ReorderQty = dto.ReorderQty,
            Barcode = dto.Barcode,
            WeightKg = dto.WeightKg,
            Images = images
        };
        var result = await _mediator.Send(command, ct);
        return result.Success ? Created(string.Empty, new ApiResponse<ProductDto>(true, result.Data!, result.Message)) : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message));
    }

    [HasPermission("Products.Edit")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto, CancellationToken ct)
    {
        var command = new UpdateProductCommand
        {
            Id = id,
            Name = dto.Name,
            Description = dto.Description,
            UnitPrice = dto.UnitPrice,
            CostPrice = dto.CostPrice,
            ReorderLevel = dto.ReorderLevel,
            ReorderQty = dto.ReorderQty,
            WeightKg = dto.WeightKg
        };
        var result = await _mediator.Send(command, ct);
        return result.Success ? Ok(new ApiResponse<ProductDto>(true, result.Data!, result.Message)) : NotFound(new ApiResponse<object>(false, result.Message!));
    }

    [HasPermission("Products.Delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteProductCommand(id), ct);
        return result.Success ? Ok(new ApiResponse<bool>(true, result.Data!, result.Message)) : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message));
    }

    [HasPermission("Products.Edit")]
    [HttpPost("{id:guid}/images")]
    public async Task<IActionResult> UploadImages(Guid id, List<IFormFile> images, CancellationToken ct)
    {
        var command = new UploadProductImagesCommand
        {
            ProductId = id,
            Images = images
        };
        var result = await _mediator.Send(command, ct);
        return result.Success ? Ok(new ApiResponse<ProductDto>(true, result.Data!, result.Message)) : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message));
    }

    [HasPermission("Products.Edit")]
    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> DeleteImage(Guid id, Guid imageId, CancellationToken ct)
    {
        var command = new DeleteProductImageCommand
        {
            ProductId = id,
            ImageId = imageId
        };
        var result = await _mediator.Send(command, ct);
        return result.Success ? Ok(new ApiResponse<ProductDto>(true, result.Data!, result.Message)) : NotFound(new ApiResponse<object>(false, result.Message!));
    }
}
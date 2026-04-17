using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Categories.Commands.CreateCategory;
using EcommerceInventory.Application.Features.Categories.Commands.DeleteCategory;
using EcommerceInventory.Application.Features.Categories.Commands.UpdateCategory;
using EcommerceInventory.Application.Features.Categories.Commands.UploadCategoryImage;
using EcommerceInventory.Application.Features.Categories.Queries.GetAllCategories;
using EcommerceInventory.Application.Features.Categories.Queries.GetCategoryById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceInventory.API.Controllers;

[Authorize]
public class CategoriesController : BaseApiController
{
    // GET /api/v1/categories
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool includeInactive = false,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetAllCategoriesQuery { IncludeInactive = includeInactive }, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    // GET /api/v1/categories/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new GetCategoryByIdQuery { Id = id }, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    // POST /api/v1/categories
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] CreateCategoryRequestDto request,
        CancellationToken ct)
    {
        Stream? imageStream = null;
        string? fileName    = null;
        string? contentType = null;

        if (request.ImageFile != null && request.ImageFile.Length > 0)
        {
            imageStream = request.ImageFile.OpenReadStream();
            fileName    = request.ImageFile.FileName;
            contentType = request.ImageFile.ContentType;
        }

        var command = new CreateCategoryCommand
        {
            Name             = request.Name,
            Description      = request.Description,
            ParentId         = request.ParentId,
            SortOrder        = request.SortOrder,
            ImageStream      = imageStream,
            ImageFileName    = fileName,
            ImageContentType = contentType
        };

        var result = await Mediator.Send(command, ct);
        return StatusCode(201, ApiResponse<object>.Ok(result,
            "Category created successfully."));
    }

    // PUT /api/v1/categories/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateCategoryCommand command,
        CancellationToken ct)
    {
        command.Id = id;
        var result = await Mediator.Send(command, ct);
        return Ok(ApiResponse<object>.Ok(result,
            "Category updated successfully."));
    }

    // DELETE /api/v1/categories/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteCategoryCommand { Id = id }, ct);
        return Ok(ApiResponse.Ok("Category deleted successfully."));
    }

    // POST /api/v1/categories/{id}/image
    [HttpPost("{id:guid}/image")]
    public async Task<IActionResult> UploadImage(
        Guid id, IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse.Fail("File is required."));

        await using var stream = file.OpenReadStream();
        var command = new UploadCategoryImageCommand
        {
            CategoryId  = id,
            FileStream  = stream,
            FileName    = file.FileName,
            ContentType = file.ContentType
        };

        var imageUrl = await Mediator.Send(command, ct);
        return Ok(ApiResponse<object>.Ok(
            new { imageUrl }, "Image uploaded successfully."));
    }
}

public class CreateCategoryRequestDto
{
    public string   Name        { get; set; } = string.Empty;
    public string?  Description { get; set; }
    public Guid?    ParentId    { get; set; }
    public int      SortOrder   { get; set; } = 0;
    public IFormFile? ImageFile { get; set; }
}

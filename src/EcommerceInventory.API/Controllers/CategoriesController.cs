using EcommerceInventory.Application.Features.Categories.Commands.UploadCategoryImage;
using EcommerceInventory.Application.Features.Categories.Commands.UpdateCategory;
using EcommerceInventory.Application.Features.Categories.Commands.DeleteCategory;
using EcommerceInventory.Application.Features.Categories.Commands.CreateCategory;
using EcommerceInventory.Application.Features.Categories.DTOs;
using EcommerceInventory.Application.Features.Categories.Queries.GetAllCategories;
using EcommerceInventory.Application.Features.Categories.Queries.GetCategoryById;
using EcommerceInventory.API.Authorization;
using EcommerceInventory.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceInventory.API.Controllers;

[Authorize]
[Route("api/v1/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission("Categories.View")]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllCategoriesQuery(), ct);
        return Ok(new ApiResponse<IReadOnlyList<CategoryDto>>(true, result.Data!));
    }

    [HasPermission("Categories.View")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCategoryByIdQuery(id), ct);
        return result.Success ? Ok(new ApiResponse<CategoryDto>(true, result.Data!)) : NotFound(new ApiResponse<object>(false, result.Message!));
    }

    [HasPermission("Categories.Create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto, CancellationToken ct)
    {
        var command = new CreateCategoryCommand
        {
            Name = dto.Name,
            Description = dto.Description,
            ParentId = dto.ParentId
        };
        var result = await _mediator.Send(command, ct);
        return result.Success ? Created(string.Empty, new ApiResponse<CategoryDto>(true, result.Data!, result.Message)) : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message));
    }

    [HasPermission("Categories.Edit")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryDto dto, CancellationToken ct)
    {
        var command = new UpdateCategoryCommand
        {
            Id = id,
            Name = dto.Name,
            Description = dto.Description
        };
        var result = await _mediator.Send(command, ct);
        return result.Success ? Ok(new ApiResponse<CategoryDto>(true, result.Data!, result.Message)) : NotFound(new ApiResponse<object>(false, result.Message!));
    }

    [HasPermission("Categories.Delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteCategoryCommand(id), ct);
        return result.Success ? Ok(new ApiResponse<bool>(true, result.Data!, result.Message)) : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message));
    }

    [HasPermission("Categories.Edit")]
    [HttpPost("{id:guid}/image")]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile imageFile, CancellationToken ct)
    {
        var command = new UploadCategoryImageCommand
        {
            CategoryId = id,
            ImageFile = imageFile
        };
        var result = await _mediator.Send(command, ct);
        return result.Success ? Ok(new ApiResponse<CategoryDto>(true, result.Data!, result.Message)) : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message));
    }
}
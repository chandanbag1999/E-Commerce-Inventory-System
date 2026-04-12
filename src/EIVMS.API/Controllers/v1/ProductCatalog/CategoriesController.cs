using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using EIVMS.Application.Modules.ProductCatalog.Interfaces;
using EIVMS.Application.Modules.ProductCatalog.DTOs.Category;

namespace EIVMS.API.Controllers.v1.ProductCatalog;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CategoriesController(IProductService productService, IHttpContextAccessor httpContextAccessor)
    {
        _productService = productService;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _productService.GetCategoryTreeAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategory(Guid id)
    {
        var result = await _productService.GetCategoryByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Policy = "product:create")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        var result = await _productService.CreateCategoryAsync(dto);
        return result.Success ? CreatedAtAction(nameof(GetCategory), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "product:update")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CreateCategoryDto dto)
    {
        var result = await _productService.UpdateCategoryAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "product:delete")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var result = await _productService.DeleteCategoryAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/image")]
    [Authorize(Policy = "product:update")]
    public async Task<IActionResult> UploadCategoryImage(Guid id, IFormFile file)
    {
        var result = await _productService.UploadCategoryImageAsync(id, file);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("deleted")]
    [Authorize(Policy = "product:read")]
    public async Task<IActionResult> GetDeletedCategories()
    {
        var result = await _productService.GetDeletedCategoriesAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/restore")]
    [Authorize(Policy = "product:update")]
    public async Task<IActionResult> RestoreCategory(Guid id)
    {
        var result = await _productService.RestoreCategoryAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("permanent")]
    [Authorize(Policy = "product:delete")]
    public async Task<IActionResult> PermanentlyDeleteCategories([FromQuery] int monthsOld = 12)
    {
        var result = await _productService.PermanentlyDeleteOldCategoriesAsync(monthsOld);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
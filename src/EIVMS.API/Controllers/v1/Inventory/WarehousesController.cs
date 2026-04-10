using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EIVMS.Application.Modules.Inventory.Interfaces;
using EIVMS.Application.Modules.Inventory.DTOs;
using EIVMS.Domain.Enums.Inventory;

namespace EIVMS.API.Controllers.v1.Inventory;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class WarehousesController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public WarehousesController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var result = await _inventoryService.GetAllWarehousesAsync(includeInactive);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _inventoryService.GetWarehouseByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("nearby")]
    [AllowAnonymous]
    public async Task<IActionResult> GetNearby([FromQuery] double latitude, [FromQuery] double longitude, [FromQuery] double radiusKm = 50)
    {
        var result = await _inventoryService.GetWarehousesNearLocationAsync(latitude, longitude, radiusKm);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost]
    [Authorize(Policy = "inventory:warehouse:create")]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseDto dto)
    {
        var result = await _inventoryService.CreateWarehouseAsync(dto);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "inventory:warehouse:update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateWarehouseDto dto)
    {
        var result = await _inventoryService.UpdateWarehouseAsync(id, dto);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "inventory:warehouse:delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _inventoryService.DeleteWarehouseAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
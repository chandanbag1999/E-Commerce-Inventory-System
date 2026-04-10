using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EIVMS.Application.Modules.Inventory.Interfaces;
using EIVMS.Application.Modules.Inventory.DTOs;
using EIVMS.Domain.Enums.Inventory;

namespace EIVMS.API.Controllers.v1.Inventory;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AlertsController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public AlertsController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAlerts([FromQuery] Guid? warehouseId = null, [FromQuery] InventoryAlertType? type = null)
    {
        var result = await _inventoryService.GetAlertsAsync(warehouseId, type);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/read")]
    [Authorize(Policy = "inventory:alert:update")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var result = await _inventoryService.MarkAlertAsReadAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{id:guid}/resolve")]
    [Authorize(Policy = "inventory:alert:update")]
    public async Task<IActionResult> Resolve(Guid id)
    {
        var result = await _inventoryService.ResolveAlertAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("generate")]
    [Authorize(Policy = "inventory:admin")]
    public async Task<IActionResult> GenerateAlerts()
    {
        var result = await _inventoryService.GenerateStockAlertsAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EIVMS.Application.Modules.Inventory.Interfaces;
using EIVMS.Application.Modules.Inventory.DTOs;
using EIVMS.Domain.Enums.Inventory;

namespace EIVMS.API.Controllers.v1.Inventory;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet("items/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetItemById(Guid id)
    {
        var result = await _inventoryService.GetInventoryItemByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("warehouse/{warehouseId:guid}/items")]
    [AllowAnonymous]
    public async Task<IActionResult> GetItemsByWarehouse(Guid warehouseId)
    {
        var result = await _inventoryService.GetInventoryItemsByWarehouseAsync(warehouseId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("product/{productId:guid}/items")]
    [AllowAnonymous]
    public async Task<IActionResult> GetItemsByProduct(Guid productId)
    {
        var result = await _inventoryService.GetInventoryItemsByProductAsync(productId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("low-stock")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLowStock([FromQuery] Guid? warehouseId = null)
    {
        var result = await _inventoryService.GetLowStockItemsAsync(warehouseId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("out-of-stock")]
    [AllowAnonymous]
    public async Task<IActionResult> GetOutOfStock([FromQuery] Guid? warehouseId = null)
    {
        var result = await _inventoryService.GetOutOfStockItemsAsync(warehouseId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("items")]
    [Authorize(Policy = "inventory:item:create")]
    public async Task<IActionResult> CreateItem([FromBody] Guid productId, [FromQuery] string sku, [FromQuery] Guid warehouseId, [FromQuery] int initialQuantity = 0, [FromQuery] int lowStockThreshold = 10)
    {
        var result = await _inventoryService.CreateInventoryItemAsync(productId, sku, warehouseId, initialQuantity, lowStockThreshold);
        return result.Success ? CreatedAtAction(nameof(GetItemById), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    [HttpPost("items/{id:guid}/adjust")]
    [Authorize(Policy = "inventory:item:update")]
    public async Task<IActionResult> AdjustItem(Guid id, [FromQuery] int newQuantity, [FromQuery] string reason)
    {
        var result = await _inventoryService.AdjustInventoryItemAsync(id, newQuantity, reason, Guid.Empty);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("stock-in")]
    [Authorize(Policy = "inventory:stock:create")]
    public async Task<IActionResult> AddStock([FromBody] StockOperationDto dto)
    {
        var result = await _inventoryService.AddStockAsync(dto, Guid.Empty);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("stock-out")]
    [Authorize(Policy = "inventory:stock:create")]
    public async Task<IActionResult> RemoveStock([FromBody] StockOperationDto dto)
    {
        var result = await _inventoryService.RemoveStockAsync(dto, Guid.Empty);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("damaged")]
    [Authorize(Policy = "inventory:stock:create")]
    public async Task<IActionResult> RecordDamaged([FromBody] StockOperationDto dto)
    {
        var result = await _inventoryService.RecordDamagedStockAsync(dto, Guid.Empty);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("items/{inventoryItemId:guid}/movements")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMovements(Guid inventoryItemId)
    {
        var result = await _inventoryService.GetStockMovementsAsync(inventoryItemId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("reserve")]
    [Authorize(Policy = "inventory:reservation:create")]
    public async Task<IActionResult> ReserveStock([FromBody] ReserveStockDto dto)
    {
        var result = await _inventoryService.ReserveStockAsync(dto);
        return result.Success ? CreatedAtAction(nameof(GetReservation), new { code = result.Data?.ReservationCode }, result) : BadRequest(result);
    }

    [HttpGet("reservations/{code}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetReservation(string code)
    {
        var result = await _inventoryService.GetReservationByCodeAsync(code);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("reservations/{id:guid}/confirm")]
    [Authorize(Policy = "inventory:reservation:update")]
    public async Task<IActionResult> ConfirmReservation(Guid id)
    {
        var result = await _inventoryService.ConfirmReservationAsync(id, Guid.Empty);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("reservations/{id:guid}/release")]
    [Authorize(Policy = "inventory:reservation:update")]
    public async Task<IActionResult> ReleaseReservation(Guid id)
    {
        var result = await _inventoryService.ReleaseReservationAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("reservations/process-expired")]
    [Authorize(Policy = "inventory:admin")]
    public async Task<IActionResult> ProcessExpiredReservations()
    {
        var result = await _inventoryService.ProcessExpiredReservationsAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
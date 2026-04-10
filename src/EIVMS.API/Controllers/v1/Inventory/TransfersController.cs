using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EIVMS.Application.Modules.Inventory.Interfaces;
using EIVMS.Application.Modules.Inventory.DTOs;
using EIVMS.Domain.Enums.Inventory;

namespace EIVMS.API.Controllers.v1.Inventory;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TransfersController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public TransfersController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet("pending")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPendingTransfers()
    {
        var result = await _inventoryService.GetPendingTransfersAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost]
    [Authorize(Policy = "inventory:transfer:create")]
    public async Task<IActionResult> CreateTransfer([FromBody] CreateStockTransferDto dto)
    {
        var result = await _inventoryService.CreateStockTransferAsync(dto, Guid.Empty);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var transfers = await _inventoryService.GetPendingTransfersAsync();
        if (!transfers.Success)
            return NotFound();
        
        var transfer = transfers.Data?.FirstOrDefault(t => t.Id == id);
        return transfer != null ? Ok(transfer) : NotFound();
    }

    [HttpPost("{id:guid}/ship")]
    [Authorize(Policy = "inventory:transfer:update")]
    public async Task<IActionResult> ShipTransfer(Guid id)
    {
        var result = await _inventoryService.ShipTransferAsync(id, Guid.Empty);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/receive")]
    [Authorize(Policy = "inventory:transfer:update")]
    public async Task<IActionResult> ReceiveTransfer(Guid id)
    {
        var result = await _inventoryService.ReceiveTransferAsync(id, Guid.Empty);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "inventory:transfer:update")]
    public async Task<IActionResult> CancelTransfer(Guid id)
    {
        var result = await _inventoryService.CancelTransferAsync(id, Guid.Empty);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
using EcommerceInventory.API.Authorization;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Stocks.Commands.AdjustStock;
using EcommerceInventory.Application.Features.Stocks.DTOs;
using EcommerceInventory.Application.Features.Stocks.Queries.GetLowStockAlerts;
using EcommerceInventory.Application.Features.Stocks.Queries.GetStockByProduct;
using EcommerceInventory.Application.Features.Stocks.Queries.GetStockByWarehouse;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceInventory.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class StocksController : ControllerBase
{
    private readonly IMediator _mediator;

    public StocksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission("Stocks.View")]
    [HttpGet("product/{productId:guid}")]
    public async Task<IActionResult> GetByProduct(Guid productId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetStockByProductQuery(productId), ct);
        return Ok(new ApiResponse<List<StockDto>>(true, result.Data!));
    }

    [HasPermission("Stocks.View")]
    [HttpGet("warehouse/{warehouseId:guid}")]
    public async Task<IActionResult> GetByWarehouse(Guid warehouseId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetStockByWarehouseQuery(warehouseId), ct);
        return Ok(new ApiResponse<List<StockDto>>(true, result.Data!));
    }

    [HasPermission("Stocks.View")]
    [HttpGet("low-stock-alerts")]
    public async Task<IActionResult> GetLowStockAlerts(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetLowStockAlertsQuery(), ct);
        return Ok(new ApiResponse<List<LowStockAlertDto>>(true, result.Data!));
    }

    [HasPermission("Stocks.Adjust")]
    [HttpPost("adjust")]
    public async Task<IActionResult> AdjustStock([FromBody] AdjustStockDto dto, CancellationToken ct)
    {
        var command = new AdjustStockCommand
        {
            ProductId = dto.ProductId,
            WarehouseId = dto.WarehouseId,
            AdjustmentType = dto.AdjustmentType,
            Quantity = dto.Quantity,
            Reason = dto.Reason
        };
        var result = await _mediator.Send(command, ct);
        return result.Success 
            ? Ok(new ApiResponse<AdjustStockResponseDto>(true, result.Data!, result.Message)) 
            : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message));
    }
}

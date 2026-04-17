using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Stocks.Commands.AdjustStock;
using EcommerceInventory.Application.Features.Stocks.Queries.GetLowStockAlerts;
using EcommerceInventory.Application.Features.Stocks.Queries.GetStockByProduct;
using EcommerceInventory.Application.Features.Stocks.Queries.GetStockByWarehouse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceInventory.API.Controllers;

[Authorize]
public class StocksController : BaseApiController
{
    // GET /api/v1/stocks/product/{productId}
    [HttpGet("product/{productId:guid}")]
    public async Task<IActionResult> GetByProduct(
        Guid productId, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new GetStockByProductQuery { ProductId = productId }, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    // GET /api/v1/stocks/warehouse/{warehouseId}
    [HttpGet("warehouse/{warehouseId:guid}")]
    public async Task<IActionResult> GetByWarehouse(
        Guid warehouseId, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new GetStockByWarehouseQuery { WarehouseId = warehouseId }, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    // GET /api/v1/stocks/low-stock-alerts
    [HttpGet("low-stock-alerts")]
    public async Task<IActionResult> GetLowStockAlerts(
        [FromQuery] Guid? warehouseId,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new GetLowStockAlertsQuery { WarehouseId = warehouseId }, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    // POST /api/v1/stocks/adjust
    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust(
        [FromBody] AdjustStockCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return Ok(ApiResponse<object>.Ok(
            result, "Stock adjusted successfully."));
    }
}

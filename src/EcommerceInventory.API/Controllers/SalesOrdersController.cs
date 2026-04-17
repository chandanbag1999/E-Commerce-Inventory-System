using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.SalesOrders.Commands.AddSalesOrderItem;
using EcommerceInventory.Application.Features.SalesOrders.Commands.ApproveSalesOrder;
using EcommerceInventory.Application.Features.SalesOrders.Commands.CancelSalesOrder;
using EcommerceInventory.Application.Features.SalesOrders.Commands.CreateSalesOrder;
using EcommerceInventory.Application.Features.SalesOrders.Commands.DeliverSalesOrder;
using EcommerceInventory.Application.Features.SalesOrders.Commands.RemoveSalesOrderItem;
using EcommerceInventory.Application.Features.SalesOrders.Commands.ShipSalesOrder;
using EcommerceInventory.Application.Features.SalesOrders.Commands.SubmitSalesOrder;
using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using EcommerceInventory.Application.Features.SalesOrders.Queries.GetAllSalesOrders;
using EcommerceInventory.Application.Features.SalesOrders.Queries.GetSalesOrderById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceInventory.API.Controllers;

[Authorize]
public class SalesOrdersController : BaseApiController
{
    // GET /api/v1/sales-orders
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetAllSalesOrdersQuery query,
        CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    // GET /api/v1/sales-orders/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new GetSalesOrderByIdQuery { Id = id }, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    // POST /api/v1/sales-orders
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSalesOrderCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return StatusCode(201, ApiResponse<object>.Ok(
            result, "Sales order created successfully."));
    }

    // POST /api/v1/sales-orders/{id}/items
    [HttpPost("{id:guid}/items")]
    public async Task<IActionResult> AddItem(
        Guid id,
        [FromBody] AddSalesOrderItemRequest request,
        CancellationToken ct)
    {
        var command = new AddSalesOrderItemCommand
        {
            SalesOrderId = id,
            ProductId    = request.ProductId,
            Quantity     = request.Quantity,
            UnitPrice    = request.UnitPrice,
            Discount     = request.Discount
        };
        var result = await Mediator.Send(command, ct);
        return Ok(ApiResponse<object>.Ok(result, "Item added successfully."));
    }

    // DELETE /api/v1/sales-orders/{id}/items/{itemId}
    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(
        Guid id, Guid itemId, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new RemoveSalesOrderItemCommand
            {
                SalesOrderId = id,
                ItemId       = itemId
            }, ct);
        return Ok(ApiResponse<object>.Ok(result, "Item removed successfully."));
    }

    // POST /api/v1/sales-orders/{id}/submit
    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> Submit(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new SubmitSalesOrderCommand { Id = id }, ct);
        return Ok(ApiResponse<object>.Ok(
            result, "Sales order submitted successfully."));
    }

    // POST /api/v1/sales-orders/{id}/approve
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new ApproveSalesOrderCommand { Id = id }, ct);
        return Ok(ApiResponse<object>.Ok(
            result, "Sales order approved. Stock reserved."));
    }

    // POST /api/v1/sales-orders/{id}/ship
    [HttpPost("{id:guid}/ship")]
    public async Task<IActionResult> Ship(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new ShipSalesOrderCommand { Id = id }, ct);
        return Ok(ApiResponse<object>.Ok(
            result, "Sales order shipped. Stock deducted."));
    }

    // POST /api/v1/sales-orders/{id}/deliver
    [HttpPost("{id:guid}/deliver")]
    public async Task<IActionResult> Deliver(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new DeliverSalesOrderCommand { Id = id }, ct);
        return Ok(ApiResponse<object>.Ok(
            result, "Sales order marked as delivered."));
    }

    // POST /api/v1/sales-orders/{id}/cancel
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid id,
        [FromBody] CancelSalesOrderRequest? request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CancelSalesOrderCommand
            {
                Id     = id,
                Reason = request?.Reason
            }, ct);
        return Ok(ApiResponse<object>.Ok(
            result, "Sales order cancelled."));
    }
}

// Request body DTOs for controller
public class CancelSalesOrderRequest
{
    public string? Reason { get; set; }
}

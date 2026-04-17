using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.PurchaseOrders.Commands.AddPurchaseOrderItem;
using EcommerceInventory.Application.Features.PurchaseOrders.Commands.ApprovePurchaseOrder;
using EcommerceInventory.Application.Features.PurchaseOrders.Commands.CancelPurchaseOrder;
using EcommerceInventory.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;
using EcommerceInventory.Application.Features.PurchaseOrders.Commands.ReceivePurchaseOrder;
using EcommerceInventory.Application.Features.PurchaseOrders.Commands.RejectPurchaseOrder;
using EcommerceInventory.Application.Features.PurchaseOrders.Commands.RemovePurchaseOrderItem;
using EcommerceInventory.Application.Features.PurchaseOrders.Commands.SubmitPurchaseOrder;
using EcommerceInventory.Application.Features.PurchaseOrders.DTOs;
using EcommerceInventory.Application.Features.PurchaseOrders.Queries.GetAllPurchaseOrders;
using EcommerceInventory.Application.Features.PurchaseOrders.Queries.GetPurchaseOrderById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceInventory.API.Controllers;

[Authorize]
public class PurchaseOrdersController : BaseApiController
{
    // GET /api/v1/purchase-orders
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetAllPurchaseOrdersQuery query,
        CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    // GET /api/v1/purchase-orders/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new GetPurchaseOrderByIdQuery { Id = id }, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    // POST /api/v1/purchase-orders
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePurchaseOrderCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return StatusCode(201, ApiResponse<object>.Ok(
            result, "Purchase order created successfully."));
    }

    // POST /api/v1/purchase-orders/{id}/items
    [HttpPost("{id:guid}/items")]
    public async Task<IActionResult> AddItem(
        Guid id,
        [FromBody] AddPurchaseOrderItemRequest request,
        CancellationToken ct)
    {
        var command = new AddPurchaseOrderItemCommand
        {
            PurchaseOrderId = id,
            ProductId       = request.ProductId,
            QuantityOrdered = request.QuantityOrdered,
            UnitCost        = request.UnitCost
        };
        var result = await Mediator.Send(command, ct);
        return Ok(ApiResponse<object>.Ok(result, "Item added successfully."));
    }

    // DELETE /api/v1/purchase-orders/{id}/items/{itemId}
    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(
        Guid id, Guid itemId, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new RemovePurchaseOrderItemCommand
            {
                PurchaseOrderId = id,
                ItemId          = itemId
            }, ct);
        return Ok(ApiResponse<object>.Ok(result, "Item removed successfully."));
    }

    // POST /api/v1/purchase-orders/{id}/submit
    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> Submit(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new SubmitPurchaseOrderCommand { Id = id }, ct);
        return Ok(ApiResponse<object>.Ok(
            result, "Purchase order submitted successfully."));
    }

    // POST /api/v1/purchase-orders/{id}/approve
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new ApprovePurchaseOrderCommand { Id = id }, ct);
        return Ok(ApiResponse<object>.Ok(
            result, "Purchase order approved successfully."));
    }

    // POST /api/v1/purchase-orders/{id}/reject
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] RejectPurchaseOrderCommand command,
        CancellationToken ct)
    {
        command.Id = id;
        var result = await Mediator.Send(command, ct);
        return Ok(ApiResponse<object>.Ok(
            result, "Purchase order rejected."));
    }

    // POST /api/v1/purchase-orders/{id}/receive
    [HttpPost("{id:guid}/receive")]
    public async Task<IActionResult> Receive(
        Guid id,
        [FromBody] ReceivePurchaseOrderRequest request,
        CancellationToken ct)
    {
        var command = new ReceivePurchaseOrderCommand
        {
            Id    = id,
            Items = request.Items
        };
        var result = await Mediator.Send(command, ct);
        return Ok(ApiResponse<object>.Ok(
            result, "Purchase order received. Stock updated successfully."));
    }

    // POST /api/v1/purchase-orders/{id}/cancel
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid id,
        [FromBody] CancelPurchaseOrderRequest? request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CancelPurchaseOrderCommand
            {
                Id     = id,
                Reason = request?.Reason
            }, ct);
        return Ok(ApiResponse<object>.Ok(
            result, "Purchase order cancelled."));
    }
}

// Request body DTOs for controller
public class ReceivePurchaseOrderRequest
{
    public List<ReceivePurchaseOrderItemRequest> Items { get; set; } = new();
}

public class CancelPurchaseOrderRequest
{
    public string? Reason { get; set; }
}

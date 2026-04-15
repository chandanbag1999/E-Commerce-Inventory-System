using EcommerceInventory.API.Authorization;
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
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceInventory.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public PurchaseOrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission("PurchaseOrders.View")]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllPurchaseOrdersQuery(), ct);
        return Ok(new ApiResponse<List<PurchaseOrderListDto>>(true, result.Data!));
    }

    [HasPermission("PurchaseOrders.View")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPurchaseOrderByIdQuery(id), ct);
        return result.Success ? Ok(new ApiResponse<PurchaseOrderDto>(true, result.Data!)) : NotFound(new ApiResponse<object>(false, result.Message!));
    }

    [HasPermission("PurchaseOrders.Create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderDto dto, CancellationToken ct)
    {
        var command = new CreatePurchaseOrderCommand
        {
            SupplierId = dto.SupplierId,
            WarehouseId = dto.WarehouseId,
            ExpectedDeliveryAt = dto.ExpectedDeliveryAt,
            Notes = dto.Notes,
            Items = dto.Items.Select(i => new CreatePurchaseOrderItemDto(i.ProductId, i.QuantityOrdered, i.UnitCost)).ToList()
        };
        var result = await _mediator.Send(command, ct);
        return result.Success ? Created(string.Empty, new ApiResponse<PurchaseOrderDto>(true, result.Data!, result.Message)) : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message));
    }

    [HasPermission("PurchaseOrders.Create")]
    [HttpPost("{id:guid}/items")]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] AddPurchaseOrderItemDto dto, CancellationToken ct)
    {
        var command = new AddPurchaseOrderItemCommand
        {
            PurchaseOrderId = id,
            ProductId = dto.ProductId,
            QuantityOrdered = dto.QuantityOrdered,
            UnitCost = dto.UnitCost
        };
        var result = await _mediator.Send(command, ct);
        return result.Success ? Ok(new ApiResponse<PurchaseOrderDto>(true, result.Data!, result.Message)) : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message));
    }

    [HasPermission("PurchaseOrders.Create")]
    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid id, Guid itemId, CancellationToken ct)
    {
        var result = await _mediator.Send(new RemovePurchaseOrderItemCommand(id, itemId), ct);
        return result.Success ? Ok(new ApiResponse<PurchaseOrderDto>(true, result.Data!, result.Message)) : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message));
    }

    [HasPermission("PurchaseOrders.Create")]
    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> Submit(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new SubmitPurchaseOrderCommand(id), ct);
        return result.Success ? Ok(new ApiResponse<PurchaseOrderDto>(true, result.Data!, result.Message)) : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message));
    }

    [HasPermission("PurchaseOrders.Approve")]
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ApprovePurchaseOrderCommand(id), ct);
        return result.Success ? Ok(new ApiResponse<PurchaseOrderDto>(true, result.Data!, result.Message)) : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message));
    }

    [HasPermission("PurchaseOrders.Approve")]
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new RejectPurchaseOrderCommand(id), ct);
        return result.Success ? Ok(new ApiResponse<PurchaseOrderDto>(true, result.Data!, result.Message)) : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message));
    }

    [HasPermission("PurchaseOrders.Receive")]
    [HttpPost("{id:guid}/receive")]
    public async Task<IActionResult> Receive(Guid id, [FromBody] ReceivePurchaseOrderDto dto, CancellationToken ct)
    {
        var command = new ReceivePurchaseOrderCommand
        {
            PurchaseOrderId = id,
            Items = dto.Items.Select(i => new ReceivePurchaseOrderItemDto(i.ItemId, i.QuantityReceived)).ToList()
        };
        var result = await _mediator.Send(command, ct);
        return result.Success ? Ok(new ApiResponse<PurchaseOrderDto>(true, result.Data!, result.Message)) : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message));
    }

    [HasPermission("PurchaseOrders.Cancel")]
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelPurchaseOrderCommand(id), ct);
        return result.Success ? Ok(new ApiResponse<PurchaseOrderDto>(true, result.Data!, result.Message)) : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message));
    }
}

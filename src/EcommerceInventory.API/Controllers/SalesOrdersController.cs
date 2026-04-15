using EcommerceInventory.API.Authorization;
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
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceInventory.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class SalesOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public SalesOrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission("SalesOrders.View")]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllSalesOrdersQuery(), ct);
        return Ok(new ApiResponse<List<SalesOrderListDto>>(true, result.Data!));
    }

    [HasPermission("SalesOrders.View")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSalesOrderByIdQuery(id), ct);
        return result.Success ? Ok(new ApiResponse<SalesOrderDto>(true, result.Data!)) : NotFound(new ApiResponse<object>(false, result.Message!));
    }

    [HasPermission("SalesOrders.Create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSalesOrderDto dto, CancellationToken ct)
    {
        var command = new CreateSalesOrderCommand
        {
            WarehouseId = dto.WarehouseId,
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            CustomerPhone = dto.CustomerPhone,
            Notes = dto.Notes,
            ShippingAddressJson = dto.ShippingAddressJson,
            Items = dto.Items.Select(i => new CreateSalesOrderItemDto(i.ProductId, i.Quantity, i.UnitPrice, i.Discount)).ToList()
        };
        var result = await _mediator.Send(command, ct);
        return result.Success ? Created(string.Empty, new ApiResponse<SalesOrderDto>(true, result.Data!, result.Message)) : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message));
    }

    [HasPermission("SalesOrders.Create")]
    [HttpPost("{id:guid}/items")]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] AddSalesOrderItemDto dto, CancellationToken ct)
    {
        var command = new AddSalesOrderItemCommand
        {
            SalesOrderId = id,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
            Discount = dto.Discount
        };
        var result = await _mediator.Send(command, ct);
        return result.Success ? Ok(new ApiResponse<SalesOrderDto>(true, result.Data!, result.Message)) : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message));
    }

    [HasPermission("SalesOrders.Create")]
    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid id, Guid itemId, CancellationToken ct)
    {
        var result = await _mediator.Send(new RemoveSalesOrderItemCommand(id, itemId), ct);
        return result.Success ? Ok(new ApiResponse<SalesOrderDto>(true, result.Data!, result.Message)) : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message));
    }

    [HasPermission("SalesOrders.Create")]
    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> Submit(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new SubmitSalesOrderCommand(id), ct);
        return result.Success ? Ok(new ApiResponse<SalesOrderDto>(true, result.Data!, result.Message)) : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message));
    }

    [HasPermission("SalesOrders.Approve")]
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ApproveSalesOrderCommand(id), ct);
        return result.Success ? Ok(new ApiResponse<SalesOrderDto>(true, result.Data!, result.Message)) : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message));
    }

    [HasPermission("SalesOrders.Ship")]
    [HttpPost("{id:guid}/ship")]
    public async Task<IActionResult> Ship(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ShipSalesOrderCommand(id), ct);
        return result.Success ? Ok(new ApiResponse<SalesOrderDto>(true, result.Data!, result.Message)) : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message));
    }

    [HasPermission("SalesOrders.Deliver")]
    [HttpPost("{id:guid}/deliver")]
    public async Task<IActionResult> Deliver(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeliverSalesOrderCommand(id), ct);
        return result.Success ? Ok(new ApiResponse<SalesOrderDto>(true, result.Data!, result.Message)) : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message));
    }

    [HasPermission("SalesOrders.Cancel")]
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelSalesOrderCommand(id), ct);
        return result.Success ? Ok(new ApiResponse<SalesOrderDto>(true, result.Data!, result.Message)) : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message));
    }
}

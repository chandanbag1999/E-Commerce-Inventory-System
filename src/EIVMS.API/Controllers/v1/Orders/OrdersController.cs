using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EIVMS.Application.Modules.Orders.DTOs;
using EIVMS.Application.Modules.Orders.Interfaces;
using EIVMS.Application.Common.Models;
using EIVMS.Domain.Enums.Orders;

namespace EIVMS.API.Controllers.v1.Orders;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _orderService.CreateOrderAsync(dto, userId);
        return result.Success ? StatusCode(201, result) : BadRequest(result);
    }

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetOrderById(Guid orderId)
    {
        var userId = GetCurrentUserId();
        var result = await _orderService.GetOrderByIdAsync(orderId, userId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("number/{orderNumber}")]
    public async Task<IActionResult> GetOrderByNumber(string orderNumber)
    {
        var userId = GetCurrentUserId();
        var result = await _orderService.GetOrderByNumberAsync(orderNumber, userId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("my-orders")]
    public async Task<IActionResult> GetMyOrders([FromQuery] OrderFilterDto filter)
    {
        var userId = GetCurrentUserId();
        var result = await _orderService.GetMyOrdersAsync(userId, filter);
        return Ok(result);
    }

    [HttpPost("{orderId:guid}/confirm-payment")]
    public async Task<IActionResult> ConfirmPayment(Guid orderId, [FromBody] ConfirmPaymentDto dto)
    {
        var result = await _orderService.ConfirmPaymentAsync(orderId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{orderId:guid}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid orderId, [FromBody] CancelOrderDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _orderService.CancelOrderAsync(orderId, dto, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{orderId:guid}/return")]
    public async Task<IActionResult> RequestReturn(Guid orderId, [FromBody] ReturnOrderDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _orderService.RequestReturnAsync(orderId, dto, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("summary")]
    [Authorize(Policy = "orders:read")]
    public async Task<IActionResult> GetOrderSummary([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] Guid? vendorId = null)
    {
        var result = await _orderService.GetOrderSummaryAsync(fromDate, toDate, vendorId);
        return Ok(result);
    }

    [HttpGet("pricing/calculate")]
    [AllowAnonymous]
    public async Task<IActionResult> CalculatePricing([FromBody] CreateOrderDto dto)
    {
        var result = await _orderService.CalculateOrderPricingAsync(dto);
        return Ok(result);
    }

    [HttpGet]
    [Authorize(Policy = "orders:read")]
    public async Task<IActionResult> GetAllOrders([FromQuery] OrderFilterDto filter)
    {
        var result = await _orderService.GetAllOrdersAsync(filter);
        return Ok(result);
    }

    [HttpPut("{orderId:guid}/status")]
    [Authorize(Policy = "orders:update")]
    public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody] UpdateOrderStatusDto dto)
    {
        var adminUserId = GetCurrentUserId();
        var result = await _orderService.UpdateOrderStatusAsync(orderId, dto, adminUserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{orderId:guid}/ship")]
    [Authorize(Policy = "orders:update")]
    public async Task<IActionResult> ShipOrder(Guid orderId, [FromBody] ShipOrderDto dto)
    {
        var adminUserId = GetCurrentUserId();
        var result = await _orderService.ShipOrderAsync(orderId, dto, adminUserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{orderId:guid}/deliver")]
    [Authorize(Policy = "orders:update")]
    public async Task<IActionResult> MarkAsDelivered(Guid orderId)
    {
        var adminUserId = GetCurrentUserId();
        var result = await _orderService.MarkAsDeliveredAsync(orderId, adminUserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{orderId:guid}/return/{returnItemId:guid}/process")]
    [Authorize(Policy = "orders:update")]
    public async Task<IActionResult> ProcessReturn(Guid orderId, Guid returnItemId, [FromQuery] bool approve, [FromQuery] string? adminNotes = null)
    {
        var adminUserId = GetCurrentUserId();
        var result = await _orderService.ProcessReturnAsync(orderId, returnItemId, approve, adminNotes, adminUserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("uid")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
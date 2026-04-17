using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.SalesOrders.Commands.CreateSalesOrder;

public class CreateSalesOrderCommand : IRequest<SalesOrderDto>
{
    public Guid    WarehouseId   { get; set; }
    public string  CustomerName  { get; set; } = "Walk-in Customer";
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? Notes         { get; set; }
    public ShippingAddressDto? ShippingAddress { get; set; }
    public List<AddSalesOrderItemRequest> Items { get; set; } = new();
}

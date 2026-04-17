using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.SalesOrders.Commands.AddSalesOrderItem;

public class AddSalesOrderItemCommand : IRequest<SalesOrderDto>
{
    public Guid    SalesOrderId { get; set; }
    public Guid    ProductId    { get; set; }
    public int     Quantity     { get; set; }
    public decimal UnitPrice    { get; set; }
    public decimal Discount     { get; set; } = 0;
}

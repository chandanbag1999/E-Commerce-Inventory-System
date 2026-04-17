using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.SalesOrders.Commands.RemoveSalesOrderItem;

public class RemoveSalesOrderItemCommand : IRequest<SalesOrderDto>
{
    public Guid SalesOrderId { get; set; }
    public Guid ItemId       { get; set; }
}

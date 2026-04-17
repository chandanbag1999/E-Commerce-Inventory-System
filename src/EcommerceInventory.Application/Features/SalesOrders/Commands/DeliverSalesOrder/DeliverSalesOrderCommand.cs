using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.SalesOrders.Commands.DeliverSalesOrder;

public class DeliverSalesOrderCommand : IRequest<SalesOrderDto>
{
    public Guid Id { get; set; }
}

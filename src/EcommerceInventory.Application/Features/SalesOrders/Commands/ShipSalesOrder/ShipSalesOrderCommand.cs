using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.SalesOrders.Commands.ShipSalesOrder;

public class ShipSalesOrderCommand : IRequest<SalesOrderDto>
{
    public Guid Id { get; set; }
}

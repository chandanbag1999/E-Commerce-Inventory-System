using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.SalesOrders.Commands.ApproveSalesOrder;

public class ApproveSalesOrderCommand : IRequest<SalesOrderDto>
{
    public Guid Id { get; set; }
}

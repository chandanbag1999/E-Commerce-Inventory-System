using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.SalesOrders.Commands.CancelSalesOrder;

public class CancelSalesOrderCommand : IRequest<SalesOrderDto>
{
    public Guid    Id     { get; set; }
    public string? Reason { get; set; }
}

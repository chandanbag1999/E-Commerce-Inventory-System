using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.SalesOrders.Commands.SubmitSalesOrder;

public class SubmitSalesOrderCommand : IRequest<SalesOrderDto>
{
    public Guid Id { get; set; }
}

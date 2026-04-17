using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.SalesOrders.Queries.GetSalesOrderById;

public class GetSalesOrderByIdQuery : IRequest<SalesOrderDto>
{
    public Guid Id { get; set; }
}

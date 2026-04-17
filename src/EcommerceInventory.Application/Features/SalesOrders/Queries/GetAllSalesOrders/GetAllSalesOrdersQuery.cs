using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.SalesOrders.Queries.GetAllSalesOrders;

public class GetAllSalesOrdersQuery : IRequest<PagedResult<SalesOrderListDto>>
{
    public int     PageNumber    { get; set; } = 1;
    public int     PageSize      { get; set; } = 20;
    public string? Status        { get; set; }
    public Guid?   WarehouseId   { get; set; }
    public string? SearchTerm    { get; set; }
}

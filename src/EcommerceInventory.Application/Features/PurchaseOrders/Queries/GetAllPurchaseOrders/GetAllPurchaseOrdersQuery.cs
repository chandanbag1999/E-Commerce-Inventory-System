using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.PurchaseOrders.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.PurchaseOrders.Queries.GetAllPurchaseOrders;

public class GetAllPurchaseOrdersQuery : IRequest<PagedResult<PurchaseOrderListDto>>
{
    public int     PageNumber  { get; set; } = 1;
    public int     PageSize    { get; set; } = 20;
    public string? Status      { get; set; }
    public Guid?   SupplierId  { get; set; }
    public Guid?   WarehouseId { get; set; }
    public string? SearchTerm  { get; set; }
}

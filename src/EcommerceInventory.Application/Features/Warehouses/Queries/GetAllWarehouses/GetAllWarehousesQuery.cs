using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Warehouses.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Warehouses.Queries.GetAllWarehouses;

public class GetAllWarehousesQuery : IRequest<PagedResult<WarehouseListDto>>
{
    public bool?  IsActive   { get; set; }
    public string? Search    { get; set; }
    public int    PageNumber { get; set; } = 1;
    public int    PageSize   { get; set; } = 20;
}

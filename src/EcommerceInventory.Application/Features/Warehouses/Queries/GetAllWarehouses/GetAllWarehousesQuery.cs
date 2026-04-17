using EcommerceInventory.Application.Features.Warehouses.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Warehouses.Queries.GetAllWarehouses;

public class GetAllWarehousesQuery : IRequest<List<WarehouseListDto>>
{
    public bool? IsActive { get; set; }
}

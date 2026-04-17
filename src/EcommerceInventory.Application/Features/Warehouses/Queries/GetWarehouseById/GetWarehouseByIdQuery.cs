using EcommerceInventory.Application.Features.Warehouses.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Warehouses.Queries.GetWarehouseById;

public class GetWarehouseByIdQuery : IRequest<WarehouseDto>
{
    public Guid Id { get; set; }
}

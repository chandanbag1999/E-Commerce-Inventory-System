using EcommerceInventory.Application.Features.Warehouses.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Warehouses.Commands.DeactivateWarehouse;

public class DeactivateWarehouseCommand : IRequest<WarehouseDto>
{
    public Guid Id { get; set; }
}

using EcommerceInventory.Application.Features.Warehouses.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Warehouses.Commands.ActivateWarehouse;

public class ActivateWarehouseCommand : IRequest<WarehouseDto>
{
    public Guid Id { get; set; }
}

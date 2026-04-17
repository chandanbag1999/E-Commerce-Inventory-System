using MediatR;

namespace EcommerceInventory.Application.Features.Warehouses.Commands.DeleteWarehouse;

public class DeleteWarehouseCommand : IRequest
{
    public Guid Id { get; set; }
}

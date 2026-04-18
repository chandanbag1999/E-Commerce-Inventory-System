using EcommerceInventory.Application.Features.Warehouses.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Warehouses.Commands.UpdateWarehouse;

public class UpdateWarehouseCommand : IRequest<WarehouseDto>
{
    public Guid       Id        { get; set; }
    public string     Name      { get; set; } = string.Empty;
    public Guid?      ManagerId { get; set; }
    public string?    Phone     { get; set; }
    public string?    Email     { get; set; }
    public int?       Capacity  { get; set; }
    public int        Version   { get; set; }
    public AddressDto? Address  { get; set; }
}

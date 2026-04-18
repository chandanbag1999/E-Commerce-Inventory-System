using EcommerceInventory.Application.Features.Warehouses.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Warehouses.Commands.CreateWarehouse;

public class CreateWarehouseCommand : IRequest<WarehouseDto>
{
    public string     Name      { get; set; } = string.Empty;
    public string     Code      { get; set; } = string.Empty;
    public Guid?      ManagerId { get; set; }
    public string?    Phone     { get; set; }
    public string?    Email     { get; set; }
    public int?       Capacity  { get; set; }
    public AddressDto? Address  { get; set; }
}

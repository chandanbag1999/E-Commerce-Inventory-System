using EcommerceInventory.Application.Features.Suppliers.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Suppliers.Commands.CreateSupplier;

public class CreateSupplierCommand : IRequest<SupplierDto>
{
    public string  Name        { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Email       { get; set; }
    public string? Phone       { get; set; }
    public string? GstNumber   { get; set; }
    public SupplierAddressDto? Address { get; set; }
}

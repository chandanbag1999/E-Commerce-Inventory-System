using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.Suppliers.DTOs;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.ValueObjects;
using MediatR;

namespace EcommerceInventory.Application.Features.Suppliers.Commands.CreateSupplier;

public class CreateSupplierCommandHandler
    : IRequestHandler<CreateSupplierCommand, SupplierDto>
{
    private readonly IUnitOfWork _uow;

    public CreateSupplierCommandHandler(IUnitOfWork uow)
        => _uow = uow;

    public async Task<SupplierDto> Handle(
        CreateSupplierCommand request,
        CancellationToken cancellationToken)
    {
        Address? address = null;
        if (request.Address != null)
        {
            address = new Address(
                request.Address.Street  ?? string.Empty,
                request.Address.City    ?? string.Empty,
                request.Address.State   ?? string.Empty,
                request.Address.Pincode ?? string.Empty,
                request.Address.Country ?? "India");
        }

        var supplier = Supplier.Create(
            name:        request.Name,
            contactName: request.ContactName,
            email:       request.Email,
            phone:       request.Phone,
            address:     address,
            gstNumber:   request.GstNumber);

        await _uow.Suppliers.AddAsync(supplier, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return MapToDto(supplier, 0);
    }

    internal static SupplierDto MapToDto(Supplier s, int totalOrders)
        => new()
        {
            Id          = s.Id,
            Name        = s.Name,
            ContactName = s.ContactName,
            Email       = s.Email,
            Phone       = s.Phone,
            GstNumber   = s.GstNumber,
            IsActive    = s.IsActive,
            TotalOrders = totalOrders,
            Address     = s.Address == null ? null : new SupplierAddressDto
            {
                Street  = s.Address.Street,
                City    = s.Address.City,
                State   = s.Address.State,
                Pincode = s.Address.Pincode,
                Country = s.Address.Country
            },
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        };
}

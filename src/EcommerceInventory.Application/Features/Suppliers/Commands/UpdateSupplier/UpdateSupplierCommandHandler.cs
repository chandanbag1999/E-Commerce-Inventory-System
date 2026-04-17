using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.Suppliers.Commands.CreateSupplier;
using EcommerceInventory.Application.Features.Suppliers.DTOs;
using EcommerceInventory.Domain.Exceptions;
using EcommerceInventory.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Suppliers.Commands.UpdateSupplier;

public class UpdateSupplierCommandHandler
    : IRequestHandler<UpdateSupplierCommand, SupplierDto>
{
    private readonly IUnitOfWork _uow;

    public UpdateSupplierCommandHandler(IUnitOfWork uow)
        => _uow = uow;

    public async Task<SupplierDto> Handle(
        UpdateSupplierCommand request,
        CancellationToken cancellationToken)
    {
        var supplier = await _uow.Suppliers.GetByIdAsync(
            request.Id, cancellationToken);
        if (supplier == null)
            throw new NotFoundException("Supplier", request.Id);

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

        supplier.Update(
            name:        request.Name,
            contactName: request.ContactName,
            email:       request.Email,
            phone:       request.Phone,
            address:     address,
            gstNumber:   request.GstNumber);

        await _uow.SaveChangesAsync(cancellationToken);

        var totalOrders = await _uow.PurchaseOrders.Query()
            .CountAsync(po => po.SupplierId == supplier.Id, cancellationToken);

        return CreateSupplierCommandHandler.MapToDto(supplier, totalOrders);
    }
}

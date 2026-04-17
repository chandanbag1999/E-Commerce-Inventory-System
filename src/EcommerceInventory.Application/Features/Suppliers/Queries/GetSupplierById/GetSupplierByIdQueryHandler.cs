using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.Suppliers.Commands.CreateSupplier;
using EcommerceInventory.Application.Features.Suppliers.DTOs;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Suppliers.Queries.GetSupplierById;

public class GetSupplierByIdQueryHandler
    : IRequestHandler<GetSupplierByIdQuery, SupplierDto>
{
    private readonly IUnitOfWork _uow;

    public GetSupplierByIdQueryHandler(IUnitOfWork uow)
        => _uow = uow;

    public async Task<SupplierDto> Handle(
        GetSupplierByIdQuery request,
        CancellationToken cancellationToken)
    {
        var supplier = await _uow.Suppliers.Query()
            .Include(s => s.PurchaseOrders)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (supplier == null)
            throw new NotFoundException("Supplier", request.Id);

        return CreateSupplierCommandHandler.MapToDto(
            supplier, supplier.PurchaseOrders.Count);
    }
}

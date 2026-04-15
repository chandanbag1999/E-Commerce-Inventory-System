using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Suppliers.DTOs;
using EcommerceInventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Suppliers.Queries.GetSupplierById;

public record GetSupplierByIdQuery(Guid Id) : IRequest<Result<SupplierDto>>;

public class GetSupplierByIdQueryHandler : IRequestHandler<GetSupplierByIdQuery, Result<SupplierDto>>
{
    private readonly IRepository<Supplier> _supplierRepository;

    public GetSupplierByIdQueryHandler(IRepository<Supplier> supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<Result<SupplierDto>> Handle(GetSupplierByIdQuery request, CancellationToken ct)
    {
        var supplier = await _supplierRepository.Query()
            .FirstOrDefaultAsync(s => s.Id == request.Id && !s.DeletedAt.HasValue, ct);

        if (supplier == null)
            return Result<SupplierDto>.FailureResult("Supplier not found");

        var dto = MapToDto(supplier);

        return Result<SupplierDto>.SuccessResult(dto);
    }

    private static SupplierDto MapToDto(Supplier supplier)
    {
        AddressDto? addressDto = null;
        if (supplier.Address != null)
        {
            addressDto = new AddressDto(
                supplier.Address.Street,
                supplier.Address.City,
                supplier.Address.State,
                supplier.Address.Pincode,
                supplier.Address.Country
            );
        }

        return new SupplierDto(
            supplier.Id,
            supplier.Name,
            supplier.ContactName,
            supplier.Email,
            supplier.Phone,
            addressDto,
            supplier.GstNumber,
            supplier.IsActive,
            supplier.CreatedAt,
            supplier.UpdatedAt
        );
    }
}

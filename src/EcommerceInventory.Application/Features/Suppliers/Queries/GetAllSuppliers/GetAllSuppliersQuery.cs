using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Suppliers.DTOs;
using EcommerceInventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Suppliers.Queries.GetAllSuppliers;

public record GetAllSuppliersQuery : IRequest<Result<List<SupplierDto>>>;

public class GetAllSuppliersQueryHandler : IRequestHandler<GetAllSuppliersQuery, Result<List<SupplierDto>>>
{
    private readonly IRepository<Supplier> _supplierRepository;

    public GetAllSuppliersQueryHandler(IRepository<Supplier> supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<Result<List<SupplierDto>>> Handle(GetAllSuppliersQuery request, CancellationToken ct)
    {
        var suppliers = await _supplierRepository.Query()
            .Where(s => !s.DeletedAt.HasValue)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);

        var dtos = suppliers.Select(MapToDto).ToList();

        return Result<List<SupplierDto>>.SuccessResult(dtos);
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

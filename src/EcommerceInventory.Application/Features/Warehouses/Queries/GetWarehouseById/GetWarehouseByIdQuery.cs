using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Warehouses.DTOs;
using EcommerceInventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Warehouses.Queries.GetWarehouseById;

public record GetWarehouseByIdQuery(Guid Id) : IRequest<Result<WarehouseDto>>;

public class GetWarehouseByIdQueryHandler : IRequestHandler<GetWarehouseByIdQuery, Result<WarehouseDto>>
{
    private readonly IRepository<Warehouse> _warehouseRepository;

    public GetWarehouseByIdQueryHandler(IRepository<Warehouse> warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
    }

    public async Task<Result<WarehouseDto>> Handle(GetWarehouseByIdQuery request, CancellationToken ct)
    {
        var warehouse = await _warehouseRepository.Query()
            .Include(w => w.Manager)
            .FirstOrDefaultAsync(w => w.Id == request.Id && !w.DeletedAt.HasValue, ct);

        if (warehouse == null)
            return Result<WarehouseDto>.FailureResult("Warehouse not found");

        var dto = MapToDto(warehouse);

        return Result<WarehouseDto>.SuccessResult(dto);
    }

    private static WarehouseDto MapToDto(Warehouse warehouse)
    {
        AddressDto? addressDto = null;
        if (warehouse.Address != null)
        {
            addressDto = new AddressDto(
                warehouse.Address.Street,
                warehouse.Address.City,
                warehouse.Address.State,
                warehouse.Address.Pincode,
                warehouse.Address.Country
            );
        }

        return new WarehouseDto(
            warehouse.Id,
            warehouse.Name,
            warehouse.Code,
            addressDto,
            warehouse.ManagerId,
            warehouse.Manager?.FullName,
            warehouse.Phone,
            warehouse.IsActive,
            warehouse.CreatedAt,
            warehouse.UpdatedAt
        );
    }
}

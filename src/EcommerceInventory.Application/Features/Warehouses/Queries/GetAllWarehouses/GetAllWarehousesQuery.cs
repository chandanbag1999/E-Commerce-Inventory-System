using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Warehouses.DTOs;
using EcommerceInventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Warehouses.Queries.GetAllWarehouses;

public record GetAllWarehousesQuery : IRequest<Result<List<WarehouseDto>>>;

public class GetAllWarehousesQueryHandler : IRequestHandler<GetAllWarehousesQuery, Result<List<WarehouseDto>>>
{
    private readonly IRepository<Warehouse> _warehouseRepository;

    public GetAllWarehousesQueryHandler(IRepository<Warehouse> warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
    }

    public async Task<Result<List<WarehouseDto>>> Handle(GetAllWarehousesQuery request, CancellationToken ct)
    {
        var warehouses = await _warehouseRepository.Query()
            .Include(w => w.Manager)
            .Where(w => !w.DeletedAt.HasValue)
            .OrderBy(w => w.Name)
            .ToListAsync(ct);

        var dtos = warehouses.Select(MapToDto).ToList();

        return Result<List<WarehouseDto>>.SuccessResult(dtos);
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

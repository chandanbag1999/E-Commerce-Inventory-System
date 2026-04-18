using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.Warehouses.DTOs;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Warehouses.Commands.ActivateWarehouse;

public class ActivateWarehouseCommandHandler
    : IRequestHandler<ActivateWarehouseCommand, WarehouseDto>
{
    private readonly IUnitOfWork _uow;

    public ActivateWarehouseCommandHandler(IUnitOfWork uow)
        => _uow = uow;

    public async Task<WarehouseDto> Handle(
        ActivateWarehouseCommand request,
        CancellationToken cancellationToken)
    {
        var warehouse = await _uow.Warehouses.GetByIdAsync(
            request.Id, cancellationToken);
        if (warehouse == null)
            throw new NotFoundException("Warehouse", request.Id);

        warehouse.Activate();
        await _uow.SaveChangesAsync(cancellationToken);

        var stockLines = await _uow.Stocks.Query()
            .CountAsync(s => s.WarehouseId == warehouse.Id, cancellationToken);

        return new WarehouseDto
        {
            Id          = warehouse.Id,
            Name        = warehouse.Name,
            Code        = warehouse.Code,
            IsActive    = warehouse.IsActive,
            Status      = warehouse.IsActive ? "Active" : "Inactive",
            Phone       = warehouse.Phone,
            Email       = warehouse.Email,
            Capacity    = warehouse.Capacity,
            Utilization = warehouse.Capacity.HasValue && warehouse.Capacity.Value > 0
                ? Math.Round((double)stockLines / warehouse.Capacity.Value * 100, 1)
                : null,
            ManagerId      = warehouse.ManagerId,
            ManagerName    = null,
            TotalStockLines = stockLines,
            Address        = warehouse.Address == null ? null : new AddressDto
            {
                Street  = warehouse.Address.Street,
                City    = warehouse.Address.City,
                State   = warehouse.Address.State,
                Pincode = warehouse.Address.Pincode,
                Country = warehouse.Address.Country
            },
            AddressString = warehouse.Address?.ToString(),
            Version    = warehouse.Version,
            CreatedAt  = warehouse.CreatedAt,
            UpdatedAt  = warehouse.UpdatedAt
        };
    }
}

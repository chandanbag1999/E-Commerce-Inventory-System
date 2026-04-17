using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.Warehouses.DTOs;
using EcommerceInventory.Domain.Exceptions;
using EcommerceInventory.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Warehouses.Commands.UpdateWarehouse;

public class UpdateWarehouseCommandHandler
    : IRequestHandler<UpdateWarehouseCommand, WarehouseDto>
{
    private readonly IUnitOfWork _uow;

    public UpdateWarehouseCommandHandler(IUnitOfWork uow)
        => _uow = uow;

    public async Task<WarehouseDto> Handle(
        UpdateWarehouseCommand request,
        CancellationToken cancellationToken)
    {
        var warehouse = await _uow.Warehouses.GetByIdAsync(
            request.Id, cancellationToken);
        if (warehouse == null)
            throw new NotFoundException("Warehouse", request.Id);

        // Manager exists check
        string? managerName = null;
        if (request.ManagerId.HasValue)
        {
            var manager = await _uow.Users.GetByIdAsync(
                request.ManagerId.Value, cancellationToken);
            if (manager == null)
                throw new NotFoundException("User", request.ManagerId.Value);
            managerName = manager.FullName;
        }

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

        warehouse.Update(request.Name, address, request.ManagerId, request.Phone);
        await _uow.SaveChangesAsync(cancellationToken);

        var stockLines = await _uow.Stocks.Query()
            .CountAsync(s => s.WarehouseId == warehouse.Id, cancellationToken);

        return new WarehouseDto
        {
            Id          = warehouse.Id,
            Name        = warehouse.Name,
            Code        = warehouse.Code,
            IsActive    = warehouse.IsActive,
            Phone       = warehouse.Phone,
            ManagerId   = warehouse.ManagerId,
            ManagerName = managerName,
            TotalStockLines = stockLines,
            Address     = warehouse.Address == null ? null : new AddressDto
            {
                Street  = warehouse.Address.Street,
                City    = warehouse.Address.City,
                State   = warehouse.Address.State,
                Pincode = warehouse.Address.Pincode,
                Country = warehouse.Address.Country
            },
            CreatedAt = warehouse.CreatedAt,
            UpdatedAt = warehouse.UpdatedAt
        };
    }
}

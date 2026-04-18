using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.Warehouses.DTOs;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Exceptions;
using EcommerceInventory.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Warehouses.Commands.CreateWarehouse;

public class CreateWarehouseCommandHandler
    : IRequestHandler<CreateWarehouseCommand, WarehouseDto>
{
    private readonly IUnitOfWork         _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateWarehouseCommandHandler(
        IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<WarehouseDto> Handle(
        CreateWarehouseCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Code uniqueness
        var codeExists = await _uow.Warehouses.Query()
            .AnyAsync(w => w.Code == request.Code.ToUpper().Trim(),
                      cancellationToken);
        if (codeExists)
            throw new DomainException(
                $"Warehouse code '{request.Code}' already exists.");

        // 2. Manager exists check
        string? managerName = null;
        if (request.ManagerId.HasValue)
        {
            var manager = await _uow.Users.GetByIdAsync(
                request.ManagerId.Value, cancellationToken);
            if (manager == null)
                throw new NotFoundException("User", request.ManagerId.Value);
            managerName = manager.FullName;
        }

        // 3. Build Address value object
        Address? address = null;
        if (request.Address != null)
        {
            address = new Address(
                request.Address.Street,
                request.Address.City,
                request.Address.State,
                request.Address.Pincode,
                request.Address.Country);
        }

        // 4. Create entity
        var warehouse = Warehouse.Create(
            name:      request.Name,
            code:      request.Code,
            address:   address,
            managerId: request.ManagerId,
            phone:     request.Phone,
            email:     request.Email,
            capacity:  request.Capacity);

        // #8: set audit
        warehouse.CreatedBy = _currentUser.UserId;
        warehouse.UpdatedBy = _currentUser.UserId;

        await _uow.Warehouses.AddAsync(warehouse, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return MapToDto(warehouse, managerName, 0);
    }

    private static WarehouseDto MapToDto(
        Warehouse w, string? managerName, int stockLines)
        => new()
        {
            Id          = w.Id,
            Name        = w.Name,
            Code        = w.Code,
            IsActive    = w.IsActive,
            Status      = w.IsActive ? "Active" : "Inactive",
            Phone       = w.Phone,
            Email       = w.Email,
            Capacity    = w.Capacity,
            Utilization = w.Capacity.HasValue && w.Capacity.Value > 0
                ? Math.Round((double)stockLines / w.Capacity.Value * 100, 1)
                : null,
            ManagerId      = w.ManagerId,
            ManagerName    = managerName,
            TotalStockLines = stockLines,
            Address        = w.Address == null ? null : new AddressDto
            {
                Street  = w.Address.Street,
                City    = w.Address.City,
                State   = w.Address.State,
                Pincode = w.Address.Pincode,
                Country = w.Address.Country
            },
            AddressString = w.Address?.ToString(),
            Version    = w.Version,
            CreatedAt  = w.CreatedAt,
            UpdatedAt  = w.UpdatedAt
        };
}

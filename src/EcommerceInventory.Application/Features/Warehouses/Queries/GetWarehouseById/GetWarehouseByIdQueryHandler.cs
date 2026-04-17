using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.Warehouses.DTOs;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Warehouses.Queries.GetWarehouseById;

public class GetWarehouseByIdQueryHandler
    : IRequestHandler<GetWarehouseByIdQuery, WarehouseDto>
{
    private readonly IUnitOfWork _uow;

    public GetWarehouseByIdQueryHandler(IUnitOfWork uow)
        => _uow = uow;

    public async Task<WarehouseDto> Handle(
        GetWarehouseByIdQuery request,
        CancellationToken cancellationToken)
    {
        var warehouse = await _uow.Warehouses.Query()
            .Include(w => w.Manager)
            .Include(w => w.Stocks)
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        if (warehouse == null)
            throw new NotFoundException("Warehouse", request.Id);

        return new WarehouseDto
        {
            Id          = warehouse.Id,
            Name        = warehouse.Name,
            Code        = warehouse.Code,
            IsActive    = warehouse.IsActive,
            Phone       = warehouse.Phone,
            ManagerId   = warehouse.ManagerId,
            ManagerName = warehouse.Manager?.FullName,
            TotalStockLines = warehouse.Stocks.Count,
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

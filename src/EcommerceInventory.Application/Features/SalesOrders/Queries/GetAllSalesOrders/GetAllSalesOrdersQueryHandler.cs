using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using EcommerceInventory.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.SalesOrders.Queries.GetAllSalesOrders;

public class GetAllSalesOrdersQueryHandler
    : IRequestHandler<GetAllSalesOrdersQuery, PagedResult<SalesOrderListDto>>
{
    private readonly IUnitOfWork _uow;

    public GetAllSalesOrdersQueryHandler(IUnitOfWork uow)
        => _uow = uow;

    public async Task<PagedResult<SalesOrderListDto>> Handle(
        GetAllSalesOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var query = _uow.SalesOrders.Query()
            .Include(so => so.Warehouse)
            .Include(so => so.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(so =>
                so.SoNumber.ToLower().Contains(term) ||
                so.CustomerName.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<OrderStatus>(request.Status, out var status))
            query = query.Where(so => so.Status == status);

        if (request.WarehouseId.HasValue)
            query = query.Where(so => so.WarehouseId == request.WarehouseId.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(so => so.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(so => new SalesOrderListDto
        {
            Id           = so.Id,
            SoNumber     = so.SoNumber,
            Status       = so.Status.ToString(),
            CustomerName = so.CustomerName,
            TotalAmount  = so.TotalAmount,
            WarehouseName = so.Warehouse.Name,
            ItemCount    = so.Items.Count,
            CreatedAt    = so.CreatedAt
        }).ToList();

        return PagedResult<SalesOrderListDto>.Create(
            dtos, total, request.PageNumber, request.PageSize);
    }
}

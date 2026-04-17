using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.PurchaseOrders.DTOs;
using EcommerceInventory.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.PurchaseOrders.Queries.GetAllPurchaseOrders;

public class GetAllPurchaseOrdersQueryHandler
    : IRequestHandler<GetAllPurchaseOrdersQuery, PagedResult<PurchaseOrderListDto>>
{
    private readonly IUnitOfWork _uow;

    public GetAllPurchaseOrdersQueryHandler(IUnitOfWork uow)
        => _uow = uow;

    public async Task<PagedResult<PurchaseOrderListDto>> Handle(
        GetAllPurchaseOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var query = _uow.PurchaseOrders.Query()
            .Include(po => po.Supplier)
            .Include(po => po.Warehouse)
            .Include(po => po.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(po =>
                po.PoNumber.ToLower().Contains(term) ||
                po.Supplier.Name.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<OrderStatus>(request.Status, out var status))
            query = query.Where(po => po.Status == status);

        if (request.SupplierId.HasValue)
            query = query.Where(po => po.SupplierId == request.SupplierId.Value);

        if (request.WarehouseId.HasValue)
            query = query.Where(po => po.WarehouseId == request.WarehouseId.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(po => po.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(po => new PurchaseOrderListDto
        {
            Id                  = po.Id,
            PoNumber            = po.PoNumber,
            Status              = po.Status.ToString(),
            TotalAmount         = po.TotalAmount,
            SupplierName        = po.Supplier.Name,
            WarehouseName       = po.Warehouse.Name,
            ItemCount           = po.Items.Count,
            ExpectedDeliveryAt  = po.ExpectedDeliveryAt,
            CreatedAt           = po.CreatedAt
        }).ToList();

        return PagedResult<PurchaseOrderListDto>.Create(
            dtos, total, request.PageNumber, request.PageSize);
    }
}

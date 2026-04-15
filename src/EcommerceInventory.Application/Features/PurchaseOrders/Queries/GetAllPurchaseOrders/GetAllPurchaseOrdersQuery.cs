using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.PurchaseOrders.DTOs;
using EcommerceInventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.PurchaseOrders.Queries.GetAllPurchaseOrders;

public record GetAllPurchaseOrdersQuery : IRequest<Result<List<PurchaseOrderListDto>>>;

public class GetAllPurchaseOrdersQueryHandler : IRequestHandler<GetAllPurchaseOrdersQuery, Result<List<PurchaseOrderListDto>>>
{
    private readonly IRepository<PurchaseOrder> _poRepository;

    public GetAllPurchaseOrdersQueryHandler(IRepository<PurchaseOrder> poRepository)
    {
        _poRepository = poRepository;
    }

    public async Task<Result<List<PurchaseOrderListDto>>> Handle(GetAllPurchaseOrdersQuery request, CancellationToken ct)
    {
        var purchaseOrders = await _poRepository.Query()
            .Include(p => p.Supplier)
            .Include(p => p.Warehouse)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        var dtos = purchaseOrders.Select(po => new PurchaseOrderListDto(
            po.Id,
            po.PoNumber,
            po.Supplier?.Name ?? string.Empty,
            po.Warehouse?.Name ?? string.Empty,
            po.Status.ToString(),
            po.TotalAmount,
            po.CreatedAt
        )).ToList();

        return Result<List<PurchaseOrderListDto>>.SuccessResult(dtos);
    }
}

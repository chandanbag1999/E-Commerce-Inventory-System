using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.PurchaseOrders.DTOs;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;

public class CreatePurchaseOrderCommandHandler
    : IRequestHandler<CreatePurchaseOrderCommand, PurchaseOrderDto>
{
    private readonly IUnitOfWork         _uow;
    private readonly ICurrentUserService _currentUser;

    public CreatePurchaseOrderCommandHandler(
        IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<PurchaseOrderDto> Handle(
        CreatePurchaseOrderCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validate supplier
        var supplier = await _uow.Suppliers.GetByIdAsync(
            request.SupplierId, cancellationToken);
        if (supplier == null)
            throw new NotFoundException("Supplier", request.SupplierId);

        // 2. Validate warehouse
        var warehouse = await _uow.Warehouses.GetByIdAsync(
            request.WarehouseId, cancellationToken);
        if (warehouse == null)
            throw new NotFoundException("Warehouse", request.WarehouseId);

        // 3. Validate all products exist (batch)
        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var validProducts = await _uow.Products.Query()
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Name, p.Sku })
            .ToListAsync(cancellationToken);

        if (validProducts.Count != productIds.Count)
        {
            var missing = productIds
                .Except(validProducts.Select(p => p.Id))
                .ToList();
            throw new NotFoundException(
                $"Products not found: {string.Join(", ", missing)}");
        }

        // 4. Generate PO number
        var poNumber = await GeneratePoNumberAsync(cancellationToken);

        // 5. Create PO entity
        var po = PurchaseOrder.Create(
            poNumber:    poNumber,
            supplierId:  request.SupplierId,
            warehouseId: request.WarehouseId,
            createdBy:   _currentUser.UserId!.Value,
            notes:       request.Notes);

        po.SetExpectedDelivery(request.ExpectedDeliveryAt);

        await _uow.PurchaseOrders.AddAsync(po, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        // 6. Add items via domain method
        foreach (var item in request.Items)
            po.AddItem(item.ProductId, item.QuantityOrdered, item.UnitCost);

        await _uow.SaveChangesAsync(cancellationToken);

        // 7. Build response
        var productLookup = validProducts.ToDictionary(p => p.Id);
        var createdByUser = await _uow.Users.GetByIdAsync(
            _currentUser.UserId!.Value, cancellationToken);

        return MapToDto(po, supplier.Name, warehouse.Name,
            createdByUser?.FullName, null,
            po.Items.Select(i => new PurchaseOrderItemDto
            {
                Id               = i.Id,
                ProductId        = i.ProductId,
                ProductName      = productLookup.ContainsKey(i.ProductId)
                                    ? productLookup[i.ProductId].Name : string.Empty,
                ProductSku       = productLookup.ContainsKey(i.ProductId)
                                    ? productLookup[i.ProductId].Sku : string.Empty,
                QuantityOrdered  = i.QuantityOrdered,
                QuantityReceived = i.QuantityReceived,
                UnitCost         = i.UnitCost,
                TotalCost        = i.TotalCost
            }).ToList());
    }

    private async Task<string> GeneratePoNumberAsync(CancellationToken ct)
    {
        var now      = DateTime.UtcNow;
        var prefix   = $"PO-{now:yyyyMM}-";
        var lastPo   = await _uow.PurchaseOrders.Query()
            .Where(p => p.PoNumber.StartsWith(prefix))
            .OrderByDescending(p => p.PoNumber)
            .Select(p => p.PoNumber)
            .FirstOrDefaultAsync(ct);

        int sequence = 1;
        if (lastPo != null)
        {
            var parts = lastPo.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out var last))
                sequence = last + 1;
        }

        return $"{prefix}{sequence:D5}";
    }

    internal static PurchaseOrderDto MapToDto(
        PurchaseOrder po,
        string supplierName,
        string warehouseName,
        string? createdByName,
        string? approvedByName,
        List<PurchaseOrderItemDto> items)
        => new()
        {
            Id                  = po.Id,
            PoNumber            = po.PoNumber,
            Status              = po.Status.ToString(),
            TotalAmount         = po.TotalAmount,
            Notes               = po.Notes,
            SupplierId          = po.SupplierId,
            SupplierName        = supplierName,
            WarehouseId         = po.WarehouseId,
            WarehouseName       = warehouseName,
            CreatedBy           = po.CreatedBy,
            CreatedByName       = createdByName,
            ApprovedBy          = po.ApprovedBy,
            ApprovedByName      = approvedByName,
            ApprovedAt          = po.ApprovedAt,
            ExpectedDeliveryAt  = po.ExpectedDeliveryAt,
            ReceivedAt          = po.ReceivedAt,
            Items               = items,
            CreatedAt           = po.CreatedAt,
            UpdatedAt           = po.UpdatedAt
        };
}

using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Exceptions;
using EcommerceInventory.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.SalesOrders.Commands.CreateSalesOrder;

public class CreateSalesOrderCommandHandler
    : IRequestHandler<CreateSalesOrderCommand, SalesOrderDto>
{
    private readonly IUnitOfWork         _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateSalesOrderCommandHandler(
        IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<SalesOrderDto> Handle(
        CreateSalesOrderCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validate warehouse
        var warehouse = await _uow.Warehouses.GetByIdAsync(
            request.WarehouseId, cancellationToken);
        if (warehouse == null)
            throw new NotFoundException("Warehouse", request.WarehouseId);

        // 2. Validate all products exist (batch)
        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var validProducts = await _uow.Products.Query()
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Name, p.Sku })
            .ToListAsync(cancellationToken);

        if (validProducts.Count != productIds.Count)
        {
            var missing = productIds.Except(validProducts.Select(p => p.Id)).ToList();
            throw new NotFoundException(
                $"Products not found: {string.Join(", ", missing)}");
        }

        // 3. Generate SO number
        var soNumber = await GenerateSoNumberAsync(cancellationToken);

        // 4. Build shipping address
        Address? shippingAddress = null;
        if (request.ShippingAddress != null)
        {
            shippingAddress = new Address(
                request.ShippingAddress.Street  ?? string.Empty,
                request.ShippingAddress.City    ?? string.Empty,
                request.ShippingAddress.State   ?? string.Empty,
                request.ShippingAddress.Pincode ?? string.Empty,
                request.ShippingAddress.Country ?? "India");
        }

        // 5. Create SO entity
        var so = SalesOrder.Create(
            soNumber:      soNumber,
            warehouseId:   request.WarehouseId,
            createdBy:     _currentUser.UserId!.Value,
            customerName:  request.CustomerName,
            customerEmail: request.CustomerEmail,
            customerPhone: request.CustomerPhone,
            notes:         request.Notes,
            shippingAddress: shippingAddress);

        await _uow.SalesOrders.AddAsync(so, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        // 6. Add items via domain method
        foreach (var item in request.Items)
            so.AddItem(item.ProductId, item.Quantity, item.UnitPrice, item.Discount);

        await _uow.SaveChangesAsync(cancellationToken);

        // 7. Build response
        var productLookup  = validProducts.ToDictionary(p => p.Id);
        var createdByUser  = await _uow.Users.GetByIdAsync(
            _currentUser.UserId!.Value, cancellationToken);

        return MapToDto(so, warehouse.Name, createdByUser?.FullName, null,
            so.Items.Select(i => new SalesOrderItemDto
            {
                Id          = i.Id,
                ProductId   = i.ProductId,
                ProductName = productLookup.ContainsKey(i.ProductId)
                                ? productLookup[i.ProductId].Name : string.Empty,
                ProductSku  = productLookup.ContainsKey(i.ProductId)
                                ? productLookup[i.ProductId].Sku : string.Empty,
                Quantity    = i.Quantity,
                UnitPrice   = i.UnitPrice,
                Discount    = i.Discount,
                LineTotal   = i.LineTotal
            }).ToList());
    }

    private async Task<string> GenerateSoNumberAsync(CancellationToken ct)
    {
        var now    = DateTime.UtcNow;
        var prefix = $"SO-{now:yyyyMM}-";
        var lastSo = await _uow.SalesOrders.Query()
            .Where(s => s.SoNumber.StartsWith(prefix))
            .OrderByDescending(s => s.SoNumber)
            .Select(s => s.SoNumber)
            .FirstOrDefaultAsync(ct);

        int sequence = 1;
        if (lastSo != null)
        {
            var parts = lastSo.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out var last))
                sequence = last + 1;
        }

        return $"{prefix}{sequence:D5}";
    }

    internal static SalesOrderDto MapToDto(
        SalesOrder so,
        string warehouseName,
        string? createdByName,
        string? approvedByName,
        List<SalesOrderItemDto> items)
        => new()
        {
            Id            = so.Id,
            SoNumber      = so.SoNumber,
            Status        = so.Status.ToString(),
            CustomerName  = so.CustomerName,
            CustomerEmail = so.CustomerEmail,
            CustomerPhone = so.CustomerPhone,
            Subtotal      = so.Subtotal,
            TotalAmount   = so.TotalAmount,
            Notes         = so.Notes,
            WarehouseId   = so.WarehouseId,
            WarehouseName = warehouseName,
            CreatedBy     = so.CreatedBy,
            CreatedByName = createdByName,
            ApprovedBy    = so.ApprovedBy,
            ApprovedByName = approvedByName,
            ApprovedAt    = so.ApprovedAt,
            ShippedAt     = so.ShippedAt,
            DeliveredAt   = so.DeliveredAt,
            ShippingAddress = so.ShippingAddress == null ? null : new ShippingAddressDto
            {
                Street  = so.ShippingAddress.Street,
                City    = so.ShippingAddress.City,
                State   = so.ShippingAddress.State,
                Pincode = so.ShippingAddress.Pincode,
                Country = so.ShippingAddress.Country
            },
            Items     = items,
            CreatedAt = so.CreatedAt,
            UpdatedAt = so.UpdatedAt
        };
}

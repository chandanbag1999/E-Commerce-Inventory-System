using EIVMS.Application.Modules.Orders.DTOs;
using EIVMS.Application.Modules.Orders.Interfaces;
using EIVMS.Domain.Entities.Orders;
using EIVMS.Domain.Enums.Orders;
using EIVMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EIVMS.Infrastructure.Repositories.Orders;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _context.Orders
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Orders
            .IgnoreQueryFilters()
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .Include(o => o.ReturnItems)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
    {
        return await _context.Orders
            .IgnoreQueryFilters()
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .Include(o => o.ReturnItems)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
    }

    public async Task<Order?> GetByIdempotencyKeyAsync(string idempotencyKey)
    {
        return await _context.Orders
            .IgnoreQueryFilters()
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .Include(o => o.ReturnItems)
            .FirstOrDefaultAsync(o => o.IdempotencyKey == idempotencyKey);
    }

    public async Task<(List<Order> Orders, int TotalCount)> GetPagedAsync(OrderFilterDto filter)
    {
        var query = _context.Orders
            .IgnoreQueryFilters()
            .Include(o => o.Items)
            .AsQueryable();

        if (filter.UserId.HasValue)
            query = query.Where(o => o.UserId == filter.UserId.Value);

        if (filter.Status.HasValue)
            query = query.Where(o => o.Status == filter.Status.Value);

        if (filter.PaymentStatus.HasValue)
            query = query.Where(o => o.PaymentStatus == filter.PaymentStatus.Value);

        if (filter.PaymentMethod.HasValue)
            query = query.Where(o => o.PaymentMethod == filter.PaymentMethod.Value);

        if (!string.IsNullOrEmpty(filter.OrderNumber))
            query = query.Where(o => o.OrderNumber.Contains(filter.OrderNumber));

        if (!string.IsNullOrEmpty(filter.SearchQuery))
            query = query.Where(o => o.OrderNumber.Contains(filter.SearchQuery) || o.ShippingContactName.Contains(filter.SearchQuery));

        if (filter.FromDate.HasValue)
            query = query.Where(o => o.CreatedAt >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(o => o.CreatedAt <= filter.ToDate.Value);

        if (filter.MinAmount.HasValue)
            query = query.Where(o => o.TotalAmount >= filter.MinAmount.Value);

        if (filter.MaxAmount.HasValue)
            query = query.Where(o => o.TotalAmount <= filter.MaxAmount.Value);

        if (filter.VendorId.HasValue)
            query = query.Where(o => o.VendorId == filter.VendorId.Value);

        var totalCount = await query.CountAsync();

        query = filter.SortBy?.ToLower() switch
        {
            "totalamount" => filter.SortDirection == "asc" ? query.OrderBy(o => o.TotalAmount) : query.OrderByDescending(o => o.TotalAmount),
            "status" => filter.SortDirection == "asc" ? query.OrderBy(o => o.Status) : query.OrderByDescending(o => o.Status),
            "createdat" or _ => filter.SortDirection == "asc" ? query.OrderBy(o => o.CreatedAt) : query.OrderByDescending(o => o.CreatedAt)
        };

        var orders = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (orders, totalCount);
    }

    public async Task<List<Order>> GetUserOrdersAsync(Guid userId, int limit = 10)
    {
        return await _context.Orders
            .IgnoreQueryFilters()
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status, int limit = 100)
    {
        return await _context.Orders
            .IgnoreQueryFilters()
            .Include(o => o.Items)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<OrderSummaryDto> GetOrderSummaryAsync(DateTime fromDate, DateTime toDate, Guid? vendorId = null)
    {
        var query = _context.Orders
            .IgnoreQueryFilters()
            .Where(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate);

        if (vendorId.HasValue)
            query = query.Where(o => o.VendorId == vendorId.Value);

        var orders = await query.ToListAsync();

        return new OrderSummaryDto
        {
            TotalOrders = orders.Count,
            PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
            ProcessingOrders = orders.Count(o => o.Status is OrderStatus.Confirmed or OrderStatus.Processing or OrderStatus.Packed or OrderStatus.Shipped or OrderStatus.OutForDelivery),
            DeliveredOrders = orders.Count(o => o.Status == OrderStatus.Delivered),
            CancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled),
            TotalRevenue = orders.Where(o => o.PaymentStatus == PaymentStatus.Paid && o.Status != OrderStatus.Cancelled).Sum(o => o.TotalAmount),
            AverageOrderValue = orders.Any() ? orders.Average(o => o.TotalAmount) : 0,
            FromDate = fromDate,
            ToDate = toDate
        };
    }

    public async Task AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
    }

    public async Task UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        await Task.CompletedTask;
    }

    public async Task AddStatusHistoryAsync(OrderStatusHistory history)
    {
        await _context.OrderStatusHistories.AddAsync(history);
    }

    public async Task AddReturnItemAsync(OrderReturnItem returnItem)
    {
        await _context.OrderReturnItems.AddAsync(returnItem);
    }

    public async Task UpdateReturnItemAsync(OrderReturnItem returnItem)
    {
        _context.OrderReturnItems.Update(returnItem);
        await Task.CompletedTask;
    }

    public async Task<bool> IdempotencyKeyExistsAsync(string key)
    {
        return await _context.Orders.AnyAsync(o => o.IdempotencyKey == key);
    }

    public async Task<bool> OrderBelongsToUserAsync(Guid orderId, Guid userId)
    {
        return await _context.Orders.AnyAsync(o => o.Id == orderId && o.UserId == userId);
    }
}
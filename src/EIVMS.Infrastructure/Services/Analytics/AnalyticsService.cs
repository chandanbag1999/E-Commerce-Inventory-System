using System.Globalization;
using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.Analytics.DTOs;
using EIVMS.Application.Modules.Analytics.Interfaces;
using EIVMS.Domain.Entities.Inventory;
using EIVMS.Domain.Entities.Orders;
using EIVMS.Domain.Enums.Inventory;
using EIVMS.Domain.Enums.Orders;
using EIVMS.Domain.Enums.ProductCatalog;
using EIVMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EIVMS.Infrastructure.Services.Analytics;

public class AnalyticsService : IAnalyticsService
{
    private static readonly CultureInfo IndiaCulture = CultureInfo.GetCultureInfo("en-IN");

    private readonly AppDbContext _context;

    public AnalyticsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<DashboardAnalyticsDto>> GetDashboardAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1);
        var previousMonthStart = currentMonthStart.AddMonths(-1);
        var previousMonthEnd = currentMonthStart;
        var last30DaysStart = now.AddDays(-30);
        var previous30DaysStart = now.AddDays(-60);

        var paidOrdersQuery = _context.Orders
            .AsNoTracking()
            .Where(order => !order.IsDeleted && order.PaymentStatus == PaymentStatus.Paid && order.Status != OrderStatus.Cancelled);

        var activeOrdersQuery = _context.Orders
            .AsNoTracking()
            .Where(order => !order.IsDeleted);

        var revenueThis30Days = await paidOrdersQuery
            .Where(order => order.CreatedAt >= last30DaysStart)
            .SumAsync(order => (decimal?)order.TotalAmount) ?? 0m;

        var revenuePrevious30Days = await paidOrdersQuery
            .Where(order => order.CreatedAt >= previous30DaysStart && order.CreatedAt < last30DaysStart)
            .SumAsync(order => (decimal?)order.TotalAmount) ?? 0m;

        var ordersThis30Days = await activeOrdersQuery.CountAsync(order => order.CreatedAt >= last30DaysStart);
        var ordersPrevious30Days = await activeOrdersQuery.CountAsync(order => order.CreatedAt >= previous30DaysStart && order.CreatedAt < last30DaysStart);

        var totalRevenue = await paidOrdersQuery.SumAsync(order => (decimal?)order.TotalAmount) ?? 0m;
        var totalOrders = await activeOrdersQuery.CountAsync();
        var activeProducts = await _context.Products
            .AsNoTracking()
            .CountAsync(product => !product.IsDeleted && product.Status == ProductStatus.Active);
        var productsAddedThisMonth = await _context.Products
            .AsNoTracking()
            .CountAsync(product => !product.IsDeleted && product.CreatedAt >= currentMonthStart);

        var pendingDeliveries = await activeOrdersQuery.CountAsync(order =>
            order.Status == OrderStatus.Packed ||
            order.Status == OrderStatus.Shipped ||
            order.Status == OrderStatus.OutForDelivery);

        var outForDelivery = await activeOrdersQuery.CountAsync(order => order.Status == OrderStatus.OutForDelivery);

        var inventoryItems = await _context.InventoryItems
            .AsNoTracking()
            .Where(item => item.IsActive)
            .ToListAsync();

        var totalStockUnits = inventoryItems.Sum(item => item.TotalQuantity);
        var lowStockItems = inventoryItems.Count(item => item.TotalQuantity - item.ReservedQuantity - item.DamagedQuantity <= item.LowStockThreshold && item.TotalQuantity - item.ReservedQuantity - item.DamagedQuantity > 0);

        var stockMovementsToday = await _context.StockMovements
            .AsNoTracking()
            .CountAsync(movement => movement.CreatedAt >= today);

        var warehouseRows = await _context.Warehouses
            .AsNoTracking()
            .Where(warehouse => !warehouse.IsDeleted && warehouse.Status == WarehouseStatus.Active)
            .Select(warehouse => new { warehouse.Id, warehouse.Name, warehouse.MaxCapacity })
            .ToListAsync();

        var inventoryByWarehouse = inventoryItems
            .GroupBy(item => item.WarehouseId)
            .ToDictionary(group => group.Key, group => group.Sum(item => item.TotalQuantity));

        var topWarehouseUtilization = warehouseRows
            .Select(warehouse =>
            {
                var maxCapacity = warehouse.MaxCapacity;
                var utilization = maxCapacity is > 0
                    ? Math.Round((double)inventoryByWarehouse.GetValueOrDefault(warehouse.Id) / maxCapacity.Value * 100, 1)
                    : 0d;

                return new
                {
                    warehouse.Name,
                    Utilization = utilization
                };
            })
            .OrderByDescending(warehouse => warehouse.Utilization)
            .FirstOrDefault();

        var activeDeliveries = await activeOrdersQuery.CountAsync(order =>
            order.Status == OrderStatus.Shipped || order.Status == OrderStatus.OutForDelivery);

        var completedTodayQuery = paidOrdersQuery.Where(order => order.ActualDeliveryDate.HasValue && order.ActualDeliveryDate.Value >= today);
        var completedToday = await completedTodayQuery.CountAsync();
        var todayDeliveryEarnings = await completedTodayQuery.SumAsync(order => (decimal?)order.ShippingCharges) ?? 0m;

        var yesterday = today.AddDays(-1);
        var yesterdayDeliveryEarnings = await paidOrdersQuery
            .Where(order => order.ActualDeliveryDate.HasValue && order.ActualDeliveryDate.Value >= yesterday && order.ActualDeliveryDate.Value < today)
            .SumAsync(order => (decimal?)order.ShippingCharges) ?? 0m;

        var thisMonthDeliveredQuery = paidOrdersQuery.Where(order => order.ActualDeliveryDate.HasValue && order.ActualDeliveryDate.Value >= currentMonthStart);
        var thisMonthDeliveryCount = await thisMonthDeliveredQuery.CountAsync();
        var thisMonthDeliveryEarnings = await thisMonthDeliveredQuery.SumAsync(order => (decimal?)order.ShippingCharges) ?? 0m;

        var currentMonthRevenue = await paidOrdersQuery
            .Where(order => order.CreatedAt >= currentMonthStart)
            .SumAsync(order => (decimal?)order.TotalAmount) ?? 0m;

        var previousMonthRevenue = await paidOrdersQuery
            .Where(order => order.CreatedAt >= previousMonthStart && order.CreatedAt < previousMonthEnd)
            .SumAsync(order => (decimal?)order.TotalAmount) ?? 0m;

        var currentMonthOrders = await activeOrdersQuery.CountAsync(order => order.CreatedAt >= currentMonthStart);
        var previousMonthOrders = await activeOrdersQuery.CountAsync(order => order.CreatedAt >= previousMonthStart && order.CreatedAt < previousMonthEnd);

        var averageOrderValueCurrentMonth = await paidOrdersQuery
            .Where(order => order.CreatedAt >= currentMonthStart)
            .AverageAsync(order => (decimal?)order.TotalAmount) ?? 0m;

        var averageOrderValuePreviousMonth = await paidOrdersQuery
            .Where(order => order.CreatedAt >= previousMonthStart && order.CreatedAt < previousMonthEnd)
            .AverageAsync(order => (decimal?)order.TotalAmount) ?? 0m;

        var revenueOverview = await BuildRevenueOverviewAsync(now);
        var ordersThisWeek = await BuildOrdersThisWeekAsync(today);
        var salesByCategory = await BuildSalesByCategoryAsync();
        var recentActivity = await BuildRecentActivityAsync();

        var dashboard = new DashboardAnalyticsDto
        {
            Stats = new DashboardStatsGroupDto
            {
                Admin =
                [
                    BuildStat("totalRevenue", "Total Revenue", FormatCurrency(totalRevenue), FormatDelta(revenueThis30Days, revenuePrevious30Days, "vs previous 30 days"), GetTrend(revenueThis30Days, revenuePrevious30Days)),
                    BuildStat("totalOrders", "Total Orders", FormatCount(totalOrders), FormatDelta(ordersThis30Days, ordersPrevious30Days, "vs previous 30 days"), GetTrend(ordersThis30Days, ordersPrevious30Days)),
                    BuildStat("productsActive", "Products Active", FormatCount(activeProducts), productsAddedThisMonth > 0 ? $"+{productsAddedThisMonth} added this month" : "No new products this month", productsAddedThisMonth > 0 ? "positive" : "neutral"),
                    BuildStat("pendingDeliveries", "Pending Deliveries", FormatCount(pendingDeliveries), outForDelivery > 0 ? $"{outForDelivery} out for delivery" : "No active delivery runs", pendingDeliveries > 0 ? "neutral" : "positive"),
                ],
                Seller =
                [
                    BuildStat("sellerRevenue", "My Revenue", FormatCurrency(currentMonthRevenue), FormatDelta(currentMonthRevenue, previousMonthRevenue, "vs previous month"), GetTrend(currentMonthRevenue, previousMonthRevenue)),
                    BuildStat("sellerOrders", "My Orders", FormatCount(currentMonthOrders), FormatDelta(currentMonthOrders, previousMonthOrders, "vs previous month"), GetTrend(currentMonthOrders, previousMonthOrders)),
                    BuildStat("sellerProducts", "My Products", FormatCount(activeProducts), productsAddedThisMonth > 0 ? $"{productsAddedThisMonth} launched this month" : "No launches this month", productsAddedThisMonth > 0 ? "positive" : "neutral"),
                    BuildStat("sellerAverageOrderValue", "Avg. Order Value", FormatCurrency(averageOrderValueCurrentMonth), FormatDelta(averageOrderValueCurrentMonth, averageOrderValuePreviousMonth, "vs previous month"), GetTrend(averageOrderValueCurrentMonth, averageOrderValuePreviousMonth)),
                ],
                Warehouse =
                [
                    BuildStat("totalStockUnits", "Total Stock Units", FormatCount(totalStockUnits), $"{warehouseRows.Count} active warehouses", "neutral"),
                    BuildStat("lowStockItems", "Low Stock Items", FormatCount(lowStockItems), lowStockItems > 0 ? "Requires attention" : "Stock health is good", lowStockItems > 0 ? "negative" : "positive"),
                    BuildStat("stockMovementsToday", "Stock Movements", FormatCount(stockMovementsToday), "Recorded today", "neutral"),
                    BuildStat("warehouseUtilization", "Warehouse Utilization", topWarehouseUtilization != null ? $"{topWarehouseUtilization.Utilization:0.#}%" : "0%", topWarehouseUtilization != null ? topWarehouseUtilization.Name : "No capacity data", "neutral"),
                ],
                Delivery =
                [
                    BuildStat("activeDeliveries", "Active Deliveries", FormatCount(activeDeliveries), pendingDeliveries > 0 ? $"{pendingDeliveries} total in the pipeline" : "No delivery backlog", "neutral"),
                    BuildStat("completedToday", "Completed Today", FormatCount(completedToday), "Delivered today", completedToday > 0 ? "positive" : "neutral"),
                    BuildStat("todaysEarnings", "Today's Earnings", FormatCurrency(todayDeliveryEarnings), FormatDelta(todayDeliveryEarnings, yesterdayDeliveryEarnings, "vs yesterday"), GetTrend(todayDeliveryEarnings, yesterdayDeliveryEarnings)),
                    BuildStat("thisMonthEarnings", "This Month", FormatCurrency(thisMonthDeliveryEarnings), thisMonthDeliveryCount > 0 ? $"{thisMonthDeliveryCount} deliveries completed" : "No completed deliveries yet", thisMonthDeliveryCount > 0 ? "positive" : "neutral"),
                ]
            },
            RevenueOverview = revenueOverview,
            OrdersThisWeek = ordersThisWeek,
            SalesByCategory = salesByCategory,
            RecentActivity = recentActivity,
        };

        return ApiResponse<DashboardAnalyticsDto>.SuccessResponse(dashboard);
    }

    private async Task<List<RevenueOverviewPointDto>> BuildRevenueOverviewAsync(DateTime now)
    {
        var currentMonthStart = new DateTime(now.Year, now.Month, 1);
        var monthStarts = Enumerable.Range(0, 6)
            .Select(offset => currentMonthStart.AddMonths(offset - 5))
            .ToList();

        var oldestMonth = monthStarts.First();

        var rawRevenue = await _context.Orders
            .AsNoTracking()
            .Where(order => !order.IsDeleted && order.PaymentStatus == PaymentStatus.Paid && order.Status != OrderStatus.Cancelled && order.CreatedAt >= oldestMonth)
            .GroupBy(order => new { order.CreatedAt.Year, order.CreatedAt.Month })
            .Select(group => new
            {
                group.Key.Year,
                group.Key.Month,
                Revenue = group.Sum(order => order.TotalAmount)
            })
            .ToListAsync();

        return monthStarts
            .Select(monthStart =>
            {
                var revenue = rawRevenue
                    .Where(entry => entry.Year == monthStart.Year && entry.Month == monthStart.Month)
                    .Select(entry => entry.Revenue)
                    .FirstOrDefault();

                return new RevenueOverviewPointDto
                {
                    Month = monthStart.ToString("MMM", CultureInfo.InvariantCulture),
                    Revenue = Math.Round(revenue, 2)
                };
            })
            .ToList();
    }

    private async Task<List<OrderTrendPointDto>> BuildOrdersThisWeekAsync(DateTime today)
    {
        var startOfWindow = today.AddDays(-6);

        var rawCounts = await _context.Orders
            .AsNoTracking()
            .Where(order => !order.IsDeleted && order.CreatedAt >= startOfWindow)
            .GroupBy(order => order.CreatedAt.Date)
            .Select(group => new
            {
                Date = group.Key,
                Count = group.Count()
            })
            .ToListAsync();

        return Enumerable.Range(0, 7)
            .Select(offset => startOfWindow.AddDays(offset))
            .Select(day => new OrderTrendPointDto
            {
                Day = day.ToString("ddd", CultureInfo.InvariantCulture),
                Orders = rawCounts.FirstOrDefault(entry => entry.Date == day)?.Count ?? 0
            })
            .ToList();
    }

    private async Task<List<SalesByCategoryPointDto>> BuildSalesByCategoryAsync()
    {
        return await (
            from item in _context.OrderItems.AsNoTracking()
            join order in _context.Orders.AsNoTracking() on item.OrderId equals order.Id
            join product in _context.Products.AsNoTracking() on item.ProductId equals product.Id
            join category in _context.Categories.AsNoTracking() on product.CategoryId equals category.Id into categoryJoin
            from category in categoryJoin.DefaultIfEmpty()
            where !order.IsDeleted && order.PaymentStatus == PaymentStatus.Paid && order.Status != OrderStatus.Cancelled && !product.IsDeleted
            group new { item.DiscountedPrice, item.Quantity } by category != null ? category.Name : "Other" into grouped
            orderby grouped.Sum(entry => entry.DiscountedPrice * entry.Quantity) descending
            select new SalesByCategoryPointDto
            {
                Name = grouped.Key,
                Value = Math.Round(grouped.Sum(entry => entry.DiscountedPrice * entry.Quantity), 2)
            })
            .Take(5)
            .ToListAsync();
    }

    private async Task<List<DashboardActivityItemDto>> BuildRecentActivityAsync()
    {
        var recentOrders = await _context.Orders
            .AsNoTracking()
            .Where(order => !order.IsDeleted)
            .OrderByDescending(order => order.CreatedAt)
            .Take(3)
            .Select(order => new DashboardActivityItemDto
            {
                Text = $"New order {order.OrderNumber} placed by {order.ShippingContactName}",
                Type = "order",
                OccurredAt = order.CreatedAt,
            })
            .ToListAsync();

        var deliveryUpdates = await _context.Orders
            .AsNoTracking()
            .Where(order => !order.IsDeleted &&
                (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.OutForDelivery || order.Status == OrderStatus.Delivered))
            .OrderByDescending(order => order.UpdatedAt ?? order.CreatedAt)
            .Take(3)
            .Select(order => new
            {
                order.OrderNumber,
                order.Status,
                OccurredAt = order.UpdatedAt ?? order.CreatedAt,
            })
            .ToListAsync();

        var recentAlerts = await _context.InventoryAlerts
            .AsNoTracking()
            .Where(alert => !alert.IsResolved)
            .OrderByDescending(alert => alert.CreatedAt)
            .Take(3)
            .Select(alert => new DashboardActivityItemDto
            {
                Text = alert.Message,
                Type = "alert",
                OccurredAt = alert.CreatedAt,
            })
            .ToListAsync();

        var stockMovements = await _context.StockMovements
            .AsNoTracking()
            .OrderByDescending(movement => movement.CreatedAt)
            .Take(3)
            .Select(movement => new DashboardActivityItemDto
            {
                Text = $"Stock movement recorded for {movement.SKU} ({movement.Quantity} units)",
                Type = "stock",
                OccurredAt = movement.CreatedAt,
            })
            .ToListAsync();

        var combined = recentOrders
            .Concat(deliveryUpdates.Select(update => new DashboardActivityItemDto
            {
                Text = update.Status switch
                {
                    OrderStatus.Delivered => $"Delivery completed for {update.OrderNumber}",
                    OrderStatus.OutForDelivery => $"Order {update.OrderNumber} is out for delivery",
                    _ => $"Order {update.OrderNumber} was shipped"
                },
                Type = "delivery",
                OccurredAt = update.OccurredAt,
            }))
            .Concat(recentAlerts)
            .Concat(stockMovements)
            .OrderByDescending(activity => activity.OccurredAt)
            .Take(5)
            .Select(activity =>
            {
                activity.Time = FormatRelativeTime(activity.OccurredAt);
                return activity;
            })
            .ToList();

        return combined;
    }

    private static DashboardStatDto BuildStat(string key, string title, string value, string change, string changeType)
    {
        return new DashboardStatDto
        {
            Key = key,
            Title = title,
            Value = value,
            Change = change,
            ChangeType = changeType,
        };
    }

    private static string FormatCurrency(decimal value)
    {
        return string.Concat("₹", value.ToString("N0", IndiaCulture));
    }

    private static string FormatCount(int value)
    {
        return value.ToString("N0", IndiaCulture);
    }

    private static string FormatDelta(decimal current, decimal previous, string suffix)
    {
        if (previous == 0)
        {
            return current == 0 ? $"No change {suffix}" : $"New activity {suffix}";
        }

        var delta = ((current - previous) / previous) * 100;
        var sign = delta > 0 ? "+" : string.Empty;
        return $"{sign}{delta:0.0}% {suffix}";
    }

    private static string FormatDelta(int current, int previous, string suffix)
    {
        if (previous == 0)
        {
            return current == 0 ? $"No change {suffix}" : $"New activity {suffix}";
        }

        var delta = ((decimal)(current - previous) / previous) * 100;
        var sign = delta > 0 ? "+" : string.Empty;
        return $"{sign}{delta:0.0}% {suffix}";
    }

    private static string GetTrend(decimal current, decimal previous)
    {
        if (current == previous) return "neutral";
        return current > previous ? "positive" : "negative";
    }

    private static string GetTrend(int current, int previous)
    {
        if (current == previous) return "neutral";
        return current > previous ? "positive" : "negative";
    }

    private static string FormatRelativeTime(DateTime occurredAt)
    {
        var elapsed = DateTime.UtcNow - occurredAt;

        if (elapsed.TotalMinutes < 1) return "just now";
        if (elapsed.TotalMinutes < 60) return $"{Math.Max(1, (int)elapsed.TotalMinutes)} min ago";
        if (elapsed.TotalHours < 24) return $"{Math.Max(1, (int)elapsed.TotalHours)} hr ago";
        return $"{Math.Max(1, (int)elapsed.TotalDays)} day ago";
    }
}

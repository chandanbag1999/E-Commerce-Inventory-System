namespace EIVMS.Application.Modules.Analytics.DTOs;

public class DashboardAnalyticsDto
{
    public DashboardStatsGroupDto Stats { get; set; } = new();
    public List<RevenueOverviewPointDto> RevenueOverview { get; set; } = new();
    public List<OrderTrendPointDto> OrdersThisWeek { get; set; } = new();
    public List<SalesByCategoryPointDto> SalesByCategory { get; set; } = new();
    public List<DashboardActivityItemDto> RecentActivity { get; set; } = new();
}

public class DashboardStatsGroupDto
{
    public List<DashboardStatDto> Admin { get; set; } = new();
    public List<DashboardStatDto> Seller { get; set; } = new();
    public List<DashboardStatDto> Warehouse { get; set; } = new();
    public List<DashboardStatDto> Delivery { get; set; } = new();
}

public class DashboardStatDto
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Change { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;
}

public class RevenueOverviewPointDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
}

public class OrderTrendPointDto
{
    public string Day { get; set; } = string.Empty;
    public int Orders { get; set; }
}

public class SalesByCategoryPointDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public class DashboardActivityItemDto
{
    public string Text { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
}

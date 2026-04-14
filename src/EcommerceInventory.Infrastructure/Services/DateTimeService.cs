using EcommerceInventory.Application.Common.Interfaces;

namespace EcommerceInventory.Infrastructure.Services;

/// <summary>
/// UTC date/time provider for testability
/// </summary>
public class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
}

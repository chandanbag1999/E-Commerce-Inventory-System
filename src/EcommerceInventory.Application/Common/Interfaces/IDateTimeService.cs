namespace EcommerceInventory.Application.Common.Interfaces;

/// <summary>
/// Service for getting current UTC date/time (for testability)
/// </summary>
public interface IDateTimeService
{
    DateTime UtcNow { get; }
}

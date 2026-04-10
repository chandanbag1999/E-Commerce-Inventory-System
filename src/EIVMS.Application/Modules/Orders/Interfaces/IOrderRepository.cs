using EIVMS.Application.Modules.Orders.DTOs;
using EIVMS.Domain.Entities.Orders;
using EIVMS.Domain.Enums.Orders;

namespace EIVMS.Application.Modules.Orders.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<Order?> GetByIdWithDetailsAsync(Guid id);
    Task<Order?> GetByOrderNumberAsync(string orderNumber);
    Task<Order?> GetByIdempotencyKeyAsync(string idempotencyKey);
    Task<(List<Order> Orders, int TotalCount)> GetPagedAsync(OrderFilterDto filter);
    Task<List<Order>> GetUserOrdersAsync(Guid userId, int limit = 10);
    Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status, int limit = 100);
    Task<OrderSummaryDto> GetOrderSummaryAsync(DateTime fromDate, DateTime toDate, Guid? vendorId = null);
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task AddStatusHistoryAsync(OrderStatusHistory history);
    Task AddReturnItemAsync(OrderReturnItem returnItem);
    Task UpdateReturnItemAsync(OrderReturnItem returnItem);
    Task<bool> IdempotencyKeyExistsAsync(string key);
    Task<bool> OrderBelongsToUserAsync(Guid orderId, Guid userId);
}
using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.Orders.DTOs;
using EIVMS.Application.Modules.UserManagement.DTOs.User;

namespace EIVMS.Application.Modules.Orders.Interfaces;

public interface IOrderService
{
    Task<ApiResponse<OrderResponseDto>> CreateOrderAsync(CreateOrderDto dto, Guid userId);
    Task<ApiResponse<OrderResponseDto>> GetOrderByIdAsync(Guid orderId, Guid requestingUserId);
    Task<ApiResponse<OrderResponseDto>> GetOrderByNumberAsync(string orderNumber, Guid requestingUserId);
    Task<ApiResponse<PaginatedResponseDto<OrderListResponseDto>>> GetMyOrdersAsync(Guid userId, OrderFilterDto filter);
    Task<ApiResponse<bool>> CancelOrderAsync(Guid orderId, CancelOrderDto dto, Guid userId);
    Task<ApiResponse<bool>> RequestReturnAsync(Guid orderId, ReturnOrderDto dto, Guid userId);
    Task<ApiResponse<OrderResponseDto>> ConfirmPaymentAsync(Guid orderId, ConfirmPaymentDto dto);
    Task<ApiResponse<bool>> HandlePaymentFailureAsync(Guid orderId, string reason);
    Task<ApiResponse<PaginatedResponseDto<OrderListResponseDto>>> GetAllOrdersAsync(OrderFilterDto filter);
    Task<ApiResponse<OrderResponseDto>> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusDto dto, Guid adminUserId);
    Task<ApiResponse<bool>> ShipOrderAsync(Guid orderId, ShipOrderDto dto, Guid adminUserId);
    Task<ApiResponse<bool>> MarkAsDeliveredAsync(Guid orderId, Guid adminUserId);
    Task<ApiResponse<bool>> ProcessReturnAsync(Guid orderId, Guid returnItemId, bool approve, string? adminNotes, Guid adminUserId);
    Task<ApiResponse<OrderSummaryDto>> GetOrderSummaryAsync(DateTime fromDate, DateTime toDate, Guid? vendorId = null);
    Task<ApiResponse<OrderPricingDto>> CalculateOrderPricingAsync(CreateOrderDto dto);
}

public interface IInventoryIntegrationService
{
    Task<bool> IsStockAvailableAsync(string sku, int quantity);
    Task<bool> ReserveStockAsync(string sku, int quantity, string orderId, Guid userId, double? latitude = null, double? longitude = null);
    Task<(bool Success, List<string> Failures)> BulkReserveAsync(List<(string SKU, int Quantity, Guid ProductId)> items, string orderId, Guid userId, double? latitude = null, double? longitude = null);
    Task ReleaseStockAsync(string orderId);
    Task ConfirmStockAsync(string orderId);
}

public interface IProductIntegrationService
{
    Task<ProductDetailsForOrderDto?> GetProductDetailsAsync(Guid productId, Guid? variantId = null);
    Task<List<ProductDetailsForOrderDto>> GetMultipleProductDetailsAsync(List<(Guid ProductId, Guid? VariantId)> products);
}

public class ProductDetailsForOrderDto
{
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public string? ImageUrl { get; set; }
    public decimal BasePrice { get; set; }
    public decimal TaxRate { get; set; }
    public bool IsAvailable { get; set; }
    public Guid? VendorId { get; set; }
}
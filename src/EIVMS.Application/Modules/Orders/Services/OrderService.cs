using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.Orders.DTOs;
using EIVMS.Application.Modules.Orders.Interfaces;
using EIVMS.Application.Modules.Orders.Validators;
using EIVMS.Application.Modules.UserManagement.DTOs.User;
using EIVMS.Domain.Entities.Orders;
using EIVMS.Domain.Enums.Orders;
using EIVMS.Domain.Events.Orders;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EIVMS.Application.Modules.Orders.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryIntegrationService _inventoryService;
    private readonly IProductIntegrationService _productService;
    private readonly IMediator _mediator;
    private readonly IValidator<CreateOrderDto> _orderValidator;
    private readonly ILogger<OrderService> _logger;

    private const decimal FreeShippingThreshold = 499m;
    private const decimal StandardShippingCharge = 49m;

    public OrderService(
        IOrderRepository orderRepository,
        IInventoryIntegrationService inventoryService,
        IProductIntegrationService productService,
        IMediator mediator,
        IValidator<CreateOrderDto> orderValidator,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _inventoryService = inventoryService;
        _productService = productService;
        _mediator = mediator;
        _orderValidator = orderValidator;
        _logger = logger;
    }

    public async Task<ApiResponse<OrderResponseDto>> CreateOrderAsync(CreateOrderDto dto, Guid userId)
    {
        var validation = await _orderValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.Select(e => e.ErrorMessage).ToList();
            return ApiResponse<OrderResponseDto>.ErrorResponse("Validation failed", statusCode: 422, errors: errors);
        }

        var existingOrder = await _orderRepository.GetByIdempotencyKeyAsync(dto.IdempotencyKey);
        if (existingOrder != null)
        {
            return ApiResponse<OrderResponseDto>.SuccessResponse(
                await MapToDetailDtoAsync(existingOrder), "Order already exists (idempotent response)");
        }

        var productRequests = dto.Items.Select(i => (i.ProductId, i.VariantId)).ToList();
        var products = await _productService.GetMultipleProductDetailsAsync(productRequests);

        var unavailableProducts = products.Where(p => !p.IsAvailable).ToList();
        if (unavailableProducts.Any())
        {
            var skus = string.Join(", ", unavailableProducts.Select(p => p.SKU));
            return ApiResponse<OrderResponseDto>.ErrorResponse($"Following products are not available: {skus}", statusCode: 400);
        }

        var pricing = await CalculatePricingAsync(dto, products);

        var order = Order.Create(userId, dto.IdempotencyKey, dto.PaymentMethod, dto.OrderType, "INR", dto.CustomerNotes, dto.IsGift, dto.GiftMessage);

        var address = dto.NewAddress!;
        order.SetShippingAddress(address.AddressLine1, address.AddressLine2, address.City, address.State, address.Country, address.PinCode, address.ContactName, address.ContactPhone);

        var productDict = products.ToDictionary(p => (p.ProductId, p.VariantId));
        foreach (var itemDto in dto.Items)
        {
            var productKey = (itemDto.ProductId, itemDto.VariantId);
            if (!productDict.TryGetValue(productKey, out var product)) continue;

            var orderItem = OrderItem.Create(order.Id, product.ProductId, product.SKU, product.ProductName, product.BasePrice, product.BasePrice, itemDto.Quantity, product.TaxRate, product.VariantId, product.VariantName, product.ImageUrl, null, product.VendorId);
            order.AddItem(orderItem);
        }

        order.SetPricing(pricing.SubTotal, pricing.ShippingCharges, pricing.TaxAmount, pricing.DiscountAmount, dto.CouponCode, pricing.CouponDiscount);

        var inventoryItems = dto.Items.Select(i => { var product = productDict[(i.ProductId, i.VariantId)]; return (product.SKU, i.Quantity, i.ProductId); }).ToList();
        var (reserveSuccess, failures) = await _inventoryService.BulkReserveAsync(inventoryItems, order.OrderNumber, userId, dto.DeliveryLatitude, dto.DeliveryLongitude);

        if (!reserveSuccess)
        {
            return ApiResponse<OrderResponseDto>.ErrorResponse($"Insufficient stock: {string.Join(", ", failures)}", statusCode: 409);
        }

        await _orderRepository.AddAsync(order);

        await _mediator.Publish(new OrderCreatedEvent(order.Id, order.OrderNumber, userId, order.TotalAmount, order.CreatedAt));

        _logger.LogInformation("Order created: {OrderNumber} for User: {UserId}, Amount: {Amount}", order.OrderNumber, userId, order.TotalAmount);

        var createdOrder = await _orderRepository.GetByIdWithDetailsAsync(order.Id);
        return ApiResponse<OrderResponseDto>.SuccessResponse(await MapToDetailDtoAsync(createdOrder!), "Order created successfully", statusCode: 201);
    }

    public async Task<ApiResponse<OrderResponseDto>> GetOrderByIdAsync(Guid orderId, Guid requestingUserId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        if (order == null || order.IsDeleted) return ApiResponse<OrderResponseDto>.ErrorResponse("Order not found", statusCode: 404);
        if (requestingUserId != Guid.Empty && order.UserId != requestingUserId) return ApiResponse<OrderResponseDto>.ErrorResponse("Access denied", statusCode: 403);
        return ApiResponse<OrderResponseDto>.SuccessResponse(await MapToDetailDtoAsync(order));
    }

    public async Task<ApiResponse<OrderResponseDto>> GetOrderByNumberAsync(string orderNumber, Guid requestingUserId)
    {
        var order = await _orderRepository.GetByOrderNumberAsync(orderNumber);
        if (order == null || order.IsDeleted) return ApiResponse<OrderResponseDto>.ErrorResponse("Order not found", statusCode: 404);
        if (requestingUserId != Guid.Empty && order.UserId != requestingUserId) return ApiResponse<OrderResponseDto>.ErrorResponse("Access denied", statusCode: 403);
        return ApiResponse<OrderResponseDto>.SuccessResponse(await MapToDetailDtoAsync(order));
    }

    public async Task<ApiResponse<PaginatedResponseDto<OrderListResponseDto>>> GetMyOrdersAsync(Guid userId, OrderFilterDto filter)
    {
        filter.UserId = userId;
        filter.PageSize = Math.Min(filter.PageSize, 50);
        var (orders, totalCount) = await _orderRepository.GetPagedAsync(filter);
        var dtos = orders.Select(MapToListDto).ToList();
        return ApiResponse<PaginatedResponseDto<OrderListResponseDto>>.SuccessResponse(new PaginatedResponseDto<OrderListResponseDto> { Items = dtos, TotalCount = totalCount, PageNumber = filter.PageNumber, PageSize = filter.PageSize });
    }

    public async Task<ApiResponse<OrderResponseDto>> ConfirmPaymentAsync(Guid orderId, ConfirmPaymentDto dto)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        if (order == null) return ApiResponse<OrderResponseDto>.ErrorResponse("Order not found", statusCode: 404);
        if (order.Status != OrderStatus.Pending) return ApiResponse<OrderResponseDto>.ErrorResponse($"Cannot confirm payment for order in {order.Status} status");

        try
        {
            order.Confirm(dto.PaymentTransactionId, dto.GatewayResponse);
            await _orderRepository.UpdateAsync(order);
            await _inventoryService.ConfirmStockAsync(order.OrderNumber);
            await _mediator.Publish(new OrderConfirmedEvent(order.Id, order.OrderNumber, order.UserId, dto.PaymentTransactionId));
            return ApiResponse<OrderResponseDto>.SuccessResponse(await MapToDetailDtoAsync(order), "Payment confirmed, order is being processed");
        }
        catch (InvalidOperationException ex) { return ApiResponse<OrderResponseDto>.ErrorResponse(ex.Message); }
    }

    public async Task<ApiResponse<bool>> HandlePaymentFailureAsync(Guid orderId, string reason)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return ApiResponse<bool>.ErrorResponse("Order not found", statusCode: 404);
        order.MarkAsFailed(reason);
        await _orderRepository.UpdateAsync(order);
        await _inventoryService.ReleaseStockAsync(order.OrderNumber);
        await _mediator.Publish(new PaymentFailedEvent(order.Id, order.OrderNumber, order.UserId, reason));
        return ApiResponse<bool>.SuccessResponse(true, "Payment failure recorded, stock released");
    }

    public async Task<ApiResponse<bool>> CancelOrderAsync(Guid orderId, CancelOrderDto dto, Guid userId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null || order.IsDeleted) return ApiResponse<bool>.ErrorResponse("Order not found", statusCode: 404);
        if (order.UserId != userId) return ApiResponse<bool>.ErrorResponse("Access denied", statusCode: 403);

        order.Cancel(dto.Reason, dto.Notes, userId);
        await _orderRepository.UpdateAsync(order);
        await _inventoryService.ReleaseStockAsync(order.OrderNumber);
        var shouldRefund = order.PaymentStatus == PaymentStatus.Paid;
        await _mediator.Publish(new OrderCancelledEvent(order.Id, order.OrderNumber, order.UserId, dto.Reason.ToString(), shouldRefund));
        return ApiResponse<bool>.SuccessResponse(true, "Order cancelled successfully");
    }

    public async Task<ApiResponse<bool>> ShipOrderAsync(Guid orderId, ShipOrderDto dto, Guid adminUserId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return ApiResponse<bool>.ErrorResponse("Order not found", statusCode: 404);
        if (order.Status != OrderStatus.Packed) return ApiResponse<bool>.ErrorResponse("Order must be in Packed status to ship");
        order.MarkAsShipped(dto.TrackingNumber, dto.CourierName, dto.TrackingUrl, dto.EstimatedDeliveryDate, adminUserId);
        await _orderRepository.UpdateAsync(order);
        await _mediator.Publish(new OrderShippedEvent(order.Id, order.OrderNumber, order.UserId, dto.TrackingNumber, dto.CourierName, dto.TrackingUrl, dto.EstimatedDeliveryDate));
        return ApiResponse<bool>.SuccessResponse(true, "Order marked as shipped");
    }

    public async Task<ApiResponse<bool>> MarkAsDeliveredAsync(Guid orderId, Guid adminUserId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return ApiResponse<bool>.ErrorResponse("Order not found", statusCode: 404);
        order.MarkAsDelivered(adminUserId);
        await _orderRepository.UpdateAsync(order);
        await _mediator.Publish(new OrderDeliveredEvent(order.Id, order.OrderNumber, order.UserId, order.ActualDeliveryDate!.Value));
        return ApiResponse<bool>.SuccessResponse(true, "Order marked as delivered");
    }

    public async Task<ApiResponse<bool>> RequestReturnAsync(Guid orderId, ReturnOrderDto dto, Guid userId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        if (order == null) return ApiResponse<bool>.ErrorResponse("Order not found", statusCode: 404);
        if (order.UserId != userId) return ApiResponse<bool>.ErrorResponse("Access denied", statusCode: 403);
        order.RequestReturn(dto.Reason, dto.Notes, userId);
        foreach (var returnItem in dto.Items)
        {
            var orderItem = order.Items.FirstOrDefault(i => i.Id == returnItem.OrderItemId);
            if (orderItem == null) continue;
            var refundAmount = orderItem.DiscountedPrice * returnItem.Quantity;
            var returnItemEntity = OrderReturnItem.Create(orderId, returnItem.OrderItemId, dto.Reason, returnItem.Quantity, refundAmount, dto.Notes, returnItem.ProofImageUrl);
            await _orderRepository.AddReturnItemAsync(returnItemEntity);
        }
        await _orderRepository.UpdateAsync(order);
        await _mediator.Publish(new OrderReturnRequestedEvent(order.Id, order.OrderNumber, order.UserId, dto.Reason.ToString()));
        return ApiResponse<bool>.SuccessResponse(true, "Return request submitted successfully");
    }

    public async Task<ApiResponse<PaginatedResponseDto<OrderListResponseDto>>> GetAllOrdersAsync(OrderFilterDto filter)
    {
        var (orders, totalCount) = await _orderRepository.GetPagedAsync(filter);
        return ApiResponse<PaginatedResponseDto<OrderListResponseDto>>.SuccessResponse(new PaginatedResponseDto<OrderListResponseDto> { Items = orders.Select(MapToListDto).ToList(), TotalCount = totalCount, PageNumber = filter.PageNumber, PageSize = filter.PageSize });
    }

    public async Task<ApiResponse<OrderResponseDto>> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusDto dto, Guid adminUserId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        if (order == null) return ApiResponse<OrderResponseDto>.ErrorResponse("Order not found", statusCode: 404);
        try
        {
            switch (dto.NewStatus)
            {
                case OrderStatus.Processing: order.StartProcessing(adminUserId); break;
                case OrderStatus.Packed: order.MarkAsPacked(adminUserId); break;
                case OrderStatus.OutForDelivery: order.MarkOutForDelivery(adminUserId); break;
                case OrderStatus.Delivered: order.MarkAsDelivered(adminUserId); break;
                default: return ApiResponse<OrderResponseDto>.ErrorResponse($"Cannot set status to {dto.NewStatus} via this endpoint");
            }
            await _orderRepository.UpdateAsync(order);
            return ApiResponse<OrderResponseDto>.SuccessResponse(await MapToDetailDtoAsync(order), $"Order status updated to {dto.NewStatus}");
        }
        catch (InvalidOperationException ex) { return ApiResponse<OrderResponseDto>.ErrorResponse(ex.Message); }
    }

    public async Task<ApiResponse<bool>> ProcessReturnAsync(Guid orderId, Guid returnItemId, bool approve, string? adminNotes, Guid adminUserId)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        if (order == null) return ApiResponse<bool>.ErrorResponse("Order not found", statusCode: 404);
        var returnItem = order.ReturnItems.FirstOrDefault(r => r.Id == returnItemId);
        if (returnItem == null) return ApiResponse<bool>.ErrorResponse("Return item not found", statusCode: 404);
        if (approve) { returnItem.Approve(adminUserId, adminNotes); if (order.ReturnItems.All(r => r.Status == "Approved")) { order.CompleteReturn(adminUserId); order.ProcessRefund(adminUserId); } }
        else { returnItem.Reject(adminUserId, adminNotes ?? "Return rejected"); }
        await _orderRepository.UpdateReturnItemAsync(returnItem);
        await _orderRepository.UpdateAsync(order);
        return ApiResponse<bool>.SuccessResponse(true, approve ? "Return approved" : "Return rejected");
    }

    public async Task<ApiResponse<OrderSummaryDto>> GetOrderSummaryAsync(DateTime fromDate, DateTime toDate, Guid? vendorId = null)
    {
        var summary = await _orderRepository.GetOrderSummaryAsync(fromDate, toDate, vendorId);
        return ApiResponse<OrderSummaryDto>.SuccessResponse(summary);
    }

    public async Task<ApiResponse<OrderPricingDto>> CalculateOrderPricingAsync(CreateOrderDto dto)
    {
        var products = await _productService.GetMultipleProductDetailsAsync(dto.Items.Select(i => (i.ProductId, i.VariantId)).ToList());
        var pricing = await CalculatePricingAsync(dto, products);
        return ApiResponse<OrderPricingDto>.SuccessResponse(pricing);
    }

    private async Task<OrderPricingDto> CalculatePricingAsync(CreateOrderDto dto, List<ProductDetailsForOrderDto> products)
    {
        var productDict = products.ToDictionary(p => (p.ProductId, p.VariantId));
        decimal subTotal = 0, taxAmount = 0;
        foreach (var item in dto.Items)
        {
            var key = (item.ProductId, item.VariantId);
            if (!productDict.TryGetValue(key, out var product)) continue;
            subTotal += product.BasePrice * item.Quantity;
            taxAmount += (product.BasePrice * product.TaxRate / 100) * item.Quantity;
        }
        var shippingCharges = subTotal >= FreeShippingThreshold ? 0m : StandardShippingCharge;
        decimal couponDiscount = 0;
        return new OrderPricingDto { SubTotal = Math.Round(subTotal, 2), ShippingCharges = shippingCharges, TaxAmount = Math.Round(taxAmount, 2), DiscountAmount = couponDiscount, CouponDiscount = couponDiscount > 0 ? couponDiscount : null, CouponCode = dto.CouponCode, TotalAmount = Math.Round(subTotal + shippingCharges + taxAmount - couponDiscount, 2), IsFreeShipping = shippingCharges == 0 };
    }

    private async Task<OrderResponseDto> MapToDetailDtoAsync(Order order)
    {
        return new OrderResponseDto
        {
            Id = order.Id, OrderNumber = order.OrderNumber, UserId = order.UserId, Status = order.Status, PaymentStatus = order.PaymentStatus, PaymentMethod = order.PaymentMethod, OrderType = order.Type,
            SubTotal = order.SubTotal, DiscountAmount = order.DiscountAmount, ShippingCharges = order.ShippingCharges, TaxAmount = order.TaxAmount, TotalAmount = order.TotalAmount, Currency = order.Currency, CouponCode = order.CouponCode,
            ShippingAddress = order.ShippingAddressLine1, ShippingCity = order.ShippingCity, ShippingState = order.ShippingState, ShippingContactName = order.ShippingContactName, ShippingContactPhone = order.ShippingContactPhone,
            TrackingNumber = order.TrackingNumber, CourierName = order.CourierName, TrackingUrl = order.TrackingUrl, EstimatedDeliveryDate = order.EstimatedDeliveryDate, ActualDeliveryDate = order.ActualDeliveryDate,
            InvoiceNumber = order.InvoiceNumber, InvoiceUrl = order.InvoiceUrl, IsGift = order.IsGift, GiftMessage = order.GiftMessage, CustomerNotes = order.CustomerNotes,
            CanBeCancelled = order.CanBeCancelled, TotalItems = order.Items.Sum(i => i.Quantity), CreatedAt = order.CreatedAt, UpdatedAt = order.UpdatedAt,
            Items = order.Items.Select(i => new OrderItemResponseDto { Id = i.Id, ProductId = i.ProductId, SKU = i.SKU, ProductName = i.ProductName, VariantName = i.VariantName, ProductImageUrl = i.ProductImageUrl, UnitPrice = i.UnitPrice, DiscountedPrice = i.DiscountedPrice, Quantity = i.Quantity, TaxRate = i.TaxRate, TotalPrice = i.TotalPrice, WarehouseName = i.WarehouseName, ReturnedQuantity = i.ReturnedQuantity, IsFullyReturned = i.IsFullyReturned }).ToList(),
            StatusHistory = order.StatusHistory.Where(h => h.IsCustomerVisible).OrderByDescending(h => h.CreatedAt).Select(h => new OrderStatusHistoryDto { FromStatus = h.FromStatus.ToString(), ToStatus = h.ToStatus.ToString(), Notes = h.Notes, Timestamp = h.CreatedAt, IsCustomerVisible = h.IsCustomerVisible }).ToList()
        };
    }

    private static OrderListResponseDto MapToListDto(Order order)
    {
        var primaryImage = order.Items.FirstOrDefault()?.ProductImageUrl;
        return new OrderListResponseDto { Id = order.Id, OrderNumber = order.OrderNumber, Status = order.Status, PaymentStatus = order.PaymentStatus, TotalAmount = order.TotalAmount, TotalItems = order.Items.Sum(i => i.Quantity), PrimaryProductImage = primaryImage, TrackingNumber = order.TrackingNumber, CreatedAt = order.CreatedAt, EstimatedDeliveryDate = order.EstimatedDeliveryDate };
    }
}
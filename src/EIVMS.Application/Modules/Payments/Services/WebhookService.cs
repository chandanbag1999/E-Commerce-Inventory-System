using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.Payments.DTOs;
using EIVMS.Application.Modules.Payments.Interfaces;
using EIVMS.Domain.Enums.Payments;
using Microsoft.Extensions.Logging;

namespace EIVMS.Application.Modules.Payments.Services;

public class WebhookService : IWebhookService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentGatewayFactory _gatewayFactory;
    private readonly IWebhookIdempotencyStore _idempotencyStore;
    private readonly ILogger<WebhookService> _logger;

    private const string EventPaymentCaptured = "payment.captured";
    private const string EventChargeSucceeded = "charge.succeeded";
    private const string EventPaymentFailed = "payment.failed";
    private const string EventChargeFailed = "charge.failed";
    private const string EventRefundProcessed = "refund.processed";
    private const string EventChargeRefunded = "charge.refunded";

    public WebhookService(
        IPaymentRepository paymentRepository,
        IPaymentGatewayFactory gatewayFactory,
        IWebhookIdempotencyStore idempotencyStore,
        ILogger<WebhookService> logger)
    {
        _paymentRepository = paymentRepository;
        _gatewayFactory = gatewayFactory;
        _idempotencyStore = idempotencyStore;
        _logger = logger;
    }

    public async Task<ApiResponse<bool>> ProcessWebhookAsync(WebhookPayloadDto webhookPayload, string providerName, CancellationToken cancellationToken = default)
    {
        var eventId = $"{webhookPayload.Event}:{webhookPayload.PaymentId}:{webhookPayload.Status}";

        if (await _idempotencyStore.HasBeenProcessedAsync(eventId, cancellationToken))
        {
            _logger.LogInformation("Webhook event already processed: {EventId}", eventId);
            return ApiResponse<bool>.SuccessResponse(true, "Event already processed", 200);
        }

        if (!Guid.TryParse(webhookPayload.PaymentId, out var paymentId))
        {
            _logger.LogWarning("Invalid PaymentId in webhook: {PaymentId}", webhookPayload.PaymentId);
            return ApiResponse<bool>.ErrorResponse("Invalid Payment ID format", 400);
        }

        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
        {
            _logger.LogWarning("Payment not found for webhook: {PaymentId}", webhookPayload.PaymentId);
            await _idempotencyStore.MarkAsProcessedAsync(eventId, TimeSpan.FromDays(7), cancellationToken);
            return ApiResponse<bool>.ErrorResponse("Payment not found", 404);
        }

        var gateway = _gatewayFactory.GetGateway(payment.Provider);
        if (gateway == null)
        {
            _logger.LogError("Gateway not found for provider: {Provider}", payment.Provider);
            return ApiResponse<bool>.ErrorResponse("Payment provider not configured", 500);
        }

        if (!string.IsNullOrEmpty(webhookPayload.Signature))
        {
            var isValid = gateway.VerifyWebhookSignature(
                webhookPayload.RawBody,
                webhookPayload.Signature,
                GetWebhookSecret(payment.Provider));

            if (!isValid)
            {
                _logger.LogWarning("Invalid webhook signature for payment: {PaymentId}", webhookPayload.PaymentId);
                return ApiResponse<bool>.ErrorResponse("Invalid webhook signature", 401);
            }
        }

        try
        {
            var result = webhookPayload.Event.ToLowerInvariant() switch
            {
                EventPaymentCaptured or EventChargeSucceeded => await HandlePaymentSuccessAsync(payment, webhookPayload, cancellationToken),
                EventPaymentFailed or EventChargeFailed => await HandlePaymentFailedAsync(payment, webhookPayload, cancellationToken),
                EventRefundProcessed or EventChargeRefunded => await HandleRefundProcessedAsync(payment, webhookPayload, cancellationToken),
                _ => ApiResponse<bool>.ErrorResponse($"Unknown event type: {webhookPayload.Event}", 400)
            };

            if (result.Success)
            {
                await _idempotencyStore.MarkAsProcessedAsync(eventId, TimeSpan.FromDays(7), cancellationToken);
                payment.ProcessWebhook(webhookPayload.RawBody);
                await _paymentRepository.SaveChangesAsync(cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook for payment: {PaymentId}", webhookPayload.PaymentId);
            return ApiResponse<bool>.ErrorResponse("Error processing webhook", 500);
        }
    }

    private async Task<ApiResponse<bool>> HandlePaymentSuccessAsync(Domain.Entities.Payments.Payment payment, WebhookPayloadDto payload, CancellationToken cancellationToken)
    {
        try
        {
            payment.MarkAsSuccess(payload.PaymentId.ToString(), payload.Status);
            await _paymentRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payment marked as successful: {PaymentId}", payment.Id);
            return ApiResponse<bool>.SuccessResponse(true, "Payment processed successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid state transition for payment: {PaymentId}", payment.Id);
            return ApiResponse<bool>.ErrorResponse(ex.Message, 400);
        }
    }

    private async Task<ApiResponse<bool>> HandlePaymentFailedAsync(Domain.Entities.Payments.Payment payment, WebhookPayloadDto payload, CancellationToken cancellationToken)
    {
        try
        {
            payment.MarkAsFailed(payload.Status, $"Payment failed: {payload.Status}");
            await _paymentRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payment marked as failed: {PaymentId}", payment.Id);
            return ApiResponse<bool>.SuccessResponse(true, "Payment failure recorded");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid state transition for payment: {PaymentId}", payment.Id);
            return ApiResponse<bool>.ErrorResponse(ex.Message, 400);
        }
    }

    private async Task<ApiResponse<bool>> HandleRefundProcessedAsync(Domain.Entities.Payments.Payment payment, WebhookPayloadDto payload, CancellationToken cancellationToken)
    {
        try
        {
            payment.MarkRefundAsSuccess(payload.PaymentId.ToString());
            await _paymentRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Refund processed for payment: {PaymentId}", payment.Id);
            return ApiResponse<bool>.SuccessResponse(true, "Refund processed successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid state transition for payment: {PaymentId}", payment.Id);
            return ApiResponse<bool>.ErrorResponse(ex.Message, 400);
        }
    }

    private static string GetWebhookSecret(PaymentProvider provider) => provider switch
    {
        PaymentProvider.Razorpay => "razorpay_webhook_secret",
        PaymentProvider.Stripe => "stripe_webhook_secret",
        _ => string.Empty
    };
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.Payments.DTOs;
using EIVMS.Application.Modules.Payments.Interfaces;
using System.Security.Claims;
using System.Text;

namespace EIVMS.API.Controllers.v1.Payments;

[ApiController]
[Route("api/v1/payments")]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IWebhookService _webhookService;
    private readonly IRefundService _refundService;
    private readonly IReconciliationService _reconciliationService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentService paymentService,
        IWebhookService webhookService,
        IRefundService refundService,
        IReconciliationService reconciliationService,
        ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _webhookService = webhookService;
        _refundService = refundService;
        _reconciliationService = reconciliationService;
        _logger = logger;
    }

    [HttpPost("create")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CreatePaymentResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<>), 400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreatePayment(
        [FromBody] CreatePaymentRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid token"));

        _logger.LogInformation(
            "CreatePayment request. User={UserId}, Order={OrderId}, Amount={Amount}",
            userId, request.OrderId, request.Amount);

        var result = await _paymentService.CreatePaymentAsync(
            request, userId, cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{paymentId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PaymentStatusDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<>), 404)]
    public async Task<IActionResult> GetPaymentStatus(
        [FromRoute] Guid paymentId,
        CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetPaymentStatusAsync(
            paymentId, cancellationToken);

        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("order/{orderId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PaymentStatusDto>>), 200)]
    public async Task<IActionResult> GetPaymentsByOrder(
        [FromRoute] Guid orderId,
        CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetPaymentsByOrderIdAsync(
            orderId, cancellationToken);

        return Ok(result);
    }

    [HttpPost("webhook/{provider}")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    public async Task<IActionResult> HandleWebhook(
        [FromRoute] string provider,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);

        var signature = ExtractSignature(provider);

        if (string.IsNullOrEmpty(signature))
        {
            _logger.LogWarning("Webhook received without signature. Provider={Provider}", provider);
            return Ok();
        }

        _logger.LogInformation(
            "Webhook received from {Provider}. ContentLength={Length}",
            provider, rawBody.Length);

        var payload = ParseWebhookPayload(rawBody, signature, provider);
        if (payload is null)
        {
            _logger.LogWarning("Failed to parse webhook payload from {Provider}", provider);
            return Ok();
        }

        await _webhookService.ProcessWebhookAsync(payload, provider, cancellationToken);

        return Ok(new { received = true });
    }

    [HttpPost("refund")]
    [Authorize(Roles = "Admin,Support")]
    [ProducesResponseType(typeof(ApiResponse<RefundResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<>), 400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> InitiateRefund(
        [FromBody] RefundRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Refund initiated for Payment={PaymentId}, Amount={Amount}",
            request.PaymentId, request.Amount);

        var result = await _refundService.InitiateRefundAsync(request, cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{paymentId:guid}/refunds")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<RefundResponseDto>>), 200)]
    public async Task<IActionResult> GetRefunds(
        [FromRoute] Guid paymentId,
        CancellationToken cancellationToken)
    {
        var result = await _refundService.GetRefundsByPaymentIdAsync(
            paymentId, cancellationToken);

        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("reconcile")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<ReconciliationReportDto>), 200)]
    public async Task<IActionResult> RunReconciliation(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        _logger.LogInformation(
            "Manual reconciliation triggered. From={FromDate}, To={ToDate}",
            fromDate, toDate);

        var result = await _reconciliationService.RunReconciliationAsync(
            fromDate, toDate);

        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("sub")?.Value;

        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    private string? ExtractSignature(string provider)
    {
        return provider.ToLower() switch
        {
            "razorpay" => Request.Headers["X-Razorpay-Signature"].FirstOrDefault(),
            "stripe" => Request.Headers["Stripe-Signature"].FirstOrDefault(),
            _ => null
        };
    }

    private WebhookPayloadDto? ParseWebhookPayload(
        string rawBody,
        string signature,
        string provider)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(rawBody);
            var root = doc.RootElement;

            return provider.ToLower() switch
            {
                "razorpay" => ParseRazorpayPayload(root, rawBody, signature),
                "stripe" => ParseStripePayload(root, rawBody, signature),
                _ => null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing webhook payload");
            return null;
        }
    }

    private static WebhookPayloadDto ParseRazorpayPayload(
        System.Text.Json.JsonElement root,
        string rawBody,
        string signature)
    {
        var eventName = root.GetProperty("event").GetString() ?? string.Empty;
        var payload = root.GetProperty("payload");
        var payment = payload.GetProperty("payment").GetProperty("entity");

        return new WebhookPayloadDto(
            Event: eventName,
            PaymentId: payment.GetProperty("id").GetString() ?? string.Empty,
            OrderId: payment.TryGetProperty("order_id", out var orderId) ? orderId.GetString() ?? string.Empty : string.Empty,
            Status: payment.GetProperty("status").GetString() ?? string.Empty,
            Signature: signature,
            RawBody: rawBody);
    }

    private static WebhookPayloadDto ParseStripePayload(
        System.Text.Json.JsonElement root,
        string rawBody,
        string signature)
    {
        var eventName = root.GetProperty("type").GetString() ?? string.Empty;
        var dataObject = root.GetProperty("data").GetProperty("object");

        return new WebhookPayloadDto(
            Event: eventName,
            PaymentId: dataObject.GetProperty("id").GetString() ?? string.Empty,
            OrderId: string.Empty,
            Status: dataObject.GetProperty("status").GetString() ?? string.Empty,
            Signature: signature,
            RawBody: rawBody);
    }
}
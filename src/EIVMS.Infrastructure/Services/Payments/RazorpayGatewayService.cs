using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using EIVMS.Application.Modules.Payments.Interfaces;

namespace EIVMS.Infrastructure.Services.Payments;

public class RazorpayGatewayService : IPaymentGateway
{
    public string ProviderName => "Razorpay";

    private readonly HttpClient _httpClient;
    private readonly RazorpaySettings _settings;
    private readonly ILogger<RazorpayGatewayService> _logger;

    private const string BaseUrl = "https://api.razorpay.com/v1";

    public RazorpayGatewayService(
        HttpClient httpClient,
        IOptions<PaymentSettings> options,
        ILogger<RazorpayGatewayService> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value.Razorpay;
        _logger = logger;

        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_settings.KeyId}:{_settings.KeySecret}"));
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
    }

    public async Task<GatewayOrderResult> CreateOrderAsync(
        decimal amount,
        string currency,
        string orderId,
        Dictionary<string, string>? metadata = null)
    {
        try
        {
            var amountInSmallestUnit = (long)(amount * 100);

            var requestBody = new
            {
                amount = amountInSmallestUnit,
                currency = currency.ToUpper(),
                receipt = orderId,
                notes = metadata ?? new Dictionary<string, string>()
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation(
                "Creating Razorpay order for OrderId={OrderId}, Amount={Amount}",
                orderId, amount);

            var response = await _httpClient.PostAsync(
                $"{BaseUrl}/orders", content);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Razorpay order creation failed. Status={Status}, Body={Body}",
                    response.StatusCode, responseBody);

                return new GatewayOrderResult(false, null, null,
                    $"Razorpay API error: {response.StatusCode}");
            }

            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            var razorpayOrderId = root.GetProperty("id").GetString()!;
            var paymentUrl = $"{_settings.CheckoutBaseUrl}?order_id={razorpayOrderId}";

            _logger.LogInformation(
                "Razorpay order created. RazorpayOrderId={OrderId}", razorpayOrderId);

            return new GatewayOrderResult(true, razorpayOrderId, paymentUrl, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception creating Razorpay order");
            return new GatewayOrderResult(false, null, null, ex.Message);
        }
    }

    public bool VerifyWebhookSignature(string payload, string signature, string secret)
    {
        try
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var expectedSignature = Convert.ToHexString(hash).ToLower();

            var isValid = expectedSignature == signature.ToLower();

            if (!isValid)
            {
                _logger.LogWarning(
                    "Razorpay signature mismatch. Expected={Expected}, Got={Got}",
                    expectedSignature, signature);
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying Razorpay webhook signature");
            return false;
        }
    }

    public async Task<GatewayPaymentDetail?> FetchPaymentAsync(string providerPaymentId)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{BaseUrl}/payments/{providerPaymentId}");

            if (!response.IsSuccessStatusCode) return null;

            var body = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            return new GatewayPaymentDetail(
                ProviderPaymentId: root.GetProperty("id").GetString()!,
                Status: root.GetProperty("status").GetString()!,
                Amount: root.GetProperty("amount").GetInt64() / 100m,
                Currency: root.GetProperty("currency").GetString()!,
                CustomerEmail: root.TryGetProperty("email", out var email) ? email.GetString() : null,
                CustomerPhone: root.TryGetProperty("contact", out var contact) ? contact.GetString() : null,
                ProcessedAt: root.TryGetProperty("created_at", out var created) 
                    ? DateTimeOffset.FromUnixTimeSeconds(created.GetInt64()).UtcDateTime 
                    : null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error fetching Razorpay payment {PaymentId}", providerPaymentId);
            return null;
        }
    }

    public async Task<GatewayRefundResult> InitiateRefundAsync(
        string providerPaymentId,
        decimal amount,
        string? reason = null)
    {
        try
        {
            var amountInSmallestUnit = (long)(amount * 100);

            var requestBody = new
            {
                amount = amountInSmallestUnit,
                notes = new { reason = reason ?? "Refund requested" }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"{BaseUrl}/payments/{providerPaymentId}/refund",
                content);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Razorpay refund failed for {PaymentId}: {Body}",
                    providerPaymentId, responseBody);

                return new GatewayRefundResult(false, null,
                    "failed", $"Razorpay refund failed: {response.StatusCode}");
            }

            using var doc = JsonDocument.Parse(responseBody);
            var refundId = doc.RootElement.GetProperty("id").GetString()!;

            _logger.LogInformation(
                "Razorpay refund created. RefundId={RefundId}", refundId);

            return new GatewayRefundResult(true, refundId, "processing", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Exception initiating Razorpay refund for {PaymentId}", providerPaymentId);
            return new GatewayRefundResult(false, null, "failed", ex.Message);
        }
    }

    public async Task<List<GatewayPaymentDetail>> FetchPaymentsByDateRangeAsync(
        DateTime fromDate,
        DateTime toDate)
    {
        try
        {
            var fromTs = new DateTimeOffset(fromDate).ToUnixTimeSeconds();
            var toTs = new DateTimeOffset(toDate).ToUnixTimeSeconds();

            var url = $"{BaseUrl}/payments?from={fromTs}&to={toTs}&count=100";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return new List<GatewayPaymentDetail>();

            var body = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);

            var items = doc.RootElement.GetProperty("items");
            var results = new List<GatewayPaymentDetail>();

            foreach (var item in items.EnumerateArray())
            {
                results.Add(new GatewayPaymentDetail(
                    ProviderPaymentId: item.GetProperty("id").GetString()!,
                    Status: item.GetProperty("status").GetString()!,
                    Amount: item.GetProperty("amount").GetInt64() / 100m,
                    Currency: item.GetProperty("currency").GetString()!,
                    CustomerEmail: null,
                    CustomerPhone: null,
                    ProcessedAt: item.TryGetProperty("created_at", out var created)
                        ? DateTimeOffset.FromUnixTimeSeconds(created.GetInt64()).UtcDateTime
                        : null));
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Razorpay payments by date range");
            return new List<GatewayPaymentDetail>();
        }
    }
}

public class PaymentSettings
{
    public RazorpaySettings Razorpay { get; set; } = new();
    public StripeSettings Stripe { get; set; } = new();
}

public class RazorpaySettings
{
    public string KeyId { get; set; } = string.Empty;
    public string KeySecret { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string CheckoutBaseUrl { get; set; } = "https://checkout.razorpay.com";
}

public class StripeSettings
{
    public string PublishableKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string CheckoutBaseUrl { get; set; } = "https://checkout.stripe.com";
}
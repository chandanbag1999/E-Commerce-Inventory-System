using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using EIVMS.Application.Modules.Payments.Interfaces;
using EIVMS.Infrastructure.Services.Payments;

namespace EIVMS.Infrastructure.Services.Payments;

public class StripeGatewayService : IPaymentGateway
{
    public string ProviderName => "Stripe";

    private readonly HttpClient _httpClient;
    private readonly StripeSettings _settings;
    private readonly ILogger<StripeGatewayService> _logger;

    private const string BaseUrl = "https://api.stripe.com/v1";

    public StripeGatewayService(
        HttpClient httpClient,
        IOptions<PaymentSettings> options,
        ILogger<StripeGatewayService> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value.Stripe;
        _logger = logger;

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer", _settings.SecretKey);
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

            var formData = new Dictionary<string, string>
            {
                ["amount"] = amountInSmallestUnit.ToString(),
                ["currency"] = currency.ToLower(),
                ["metadata[internal_order_id]"] = orderId,
                ["automatic_payment_methods[enabled]"] = "true"
            };

            var content = new FormUrlEncodedContent(formData);

            var response = await _httpClient.PostAsync(
                $"{BaseUrl}/payment_intents", content);

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Stripe PI creation failed: {Body}", body);
                return new GatewayOrderResult(false, null, null,
                    "Stripe PaymentIntent creation failed");
            }

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var piId = root.GetProperty("id").GetString()!;
            var clientSecret = root.GetProperty("client_secret").GetString()!;

            var paymentUrl = $"{_settings.CheckoutBaseUrl}?client_secret={clientSecret}";

            return new GatewayOrderResult(true, piId, paymentUrl, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception creating Stripe PaymentIntent");
            return new GatewayOrderResult(false, null, null, ex.Message);
        }
    }

    public bool VerifyWebhookSignature(string payload, string signature, string secret)
    {
        try
        {
            if (string.IsNullOrEmpty(signature)) return false;

            var parts = signature.Split(',')
                .Select(p => p.Split('=', 2))
                .Where(p => p.Length == 2)
                .ToDictionary(p => p[0], p => p[1]);

            if (!parts.TryGetValue("t", out var timestampStr) ||
                !parts.TryGetValue("v1", out var expectedSig))
            {
                return false;
            }

            var signedPayload = $"{timestampStr}.{payload}";

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload));
            var computedSig = Convert.ToHexString(computedHash).ToLower();

            return computedSig == expectedSig.ToLower();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying Stripe webhook signature");
            return false;
        }
    }

    public async Task<GatewayPaymentDetail?> FetchPaymentAsync(string providerPaymentId)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{BaseUrl}/payment_intents/{providerPaymentId}");

            if (!response.IsSuccessStatusCode) return null;

            var body = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            return new GatewayPaymentDetail(
                ProviderPaymentId: root.GetProperty("id").GetString()!,
                Status: root.GetProperty("status").GetString()!,
                Amount: root.GetProperty("amount").GetInt64() / 100m,
                Currency: root.GetProperty("currency").GetString()!.ToUpper(),
                CustomerEmail: root.TryGetProperty("receipt_email", out var email) ? email.GetString() : null,
                CustomerPhone: null,
                ProcessedAt: root.TryGetProperty("created", out var created)
                    ? DateTimeOffset.FromUnixTimeSeconds(created.GetInt64()).UtcDateTime
                    : null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Stripe PI {PaymentId}", providerPaymentId);
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

            var formData = new Dictionary<string, string>
            {
                ["payment_intent"] = providerPaymentId,
                ["amount"] = amountInSmallestUnit.ToString()
            };

            var content = new FormUrlEncodedContent(formData);
            var response = await _httpClient.PostAsync(
                $"{BaseUrl}/refunds", content);

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new GatewayRefundResult(false, null,
                    "failed", $"Stripe refund failed: {response.StatusCode}");
            }

            using var doc = JsonDocument.Parse(body);
            var refundId = doc.RootElement.GetProperty("id").GetString()!;

            return new GatewayRefundResult(true, refundId, "processing", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception initiating Stripe refund");
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

            var url = $"{BaseUrl}/payment_intents?" +
                     $"created[gte]={fromTs}&created[lte]={toTs}&limit=100";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new List<GatewayPaymentDetail>();

            var body = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);

            var data = doc.RootElement.GetProperty("data");
            var results = new List<GatewayPaymentDetail>();

            foreach (var item in data.EnumerateArray())
            {
                results.Add(new GatewayPaymentDetail(
                    item.GetProperty("id").GetString()!,
                    item.GetProperty("status").GetString()!,
                    item.GetProperty("amount").GetInt64() / 100m,
                    item.GetProperty("currency").GetString()!.ToUpper(),
                    null, null,
                    item.TryGetProperty("created", out var created)
                        ? DateTimeOffset.FromUnixTimeSeconds(created.GetInt64()).UtcDateTime
                        : null));
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Stripe payments by date range");
            return new List<GatewayPaymentDetail>();
        }
    }
}
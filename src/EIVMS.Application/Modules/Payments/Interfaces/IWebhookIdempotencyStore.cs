namespace EIVMS.Application.Modules.Payments.Interfaces;

public interface IWebhookIdempotencyStore
{
    Task<bool> HasBeenProcessedAsync(string key, CancellationToken ct = default);
    Task MarkAsProcessedAsync(string key, TimeSpan ttl, CancellationToken ct = default);
}

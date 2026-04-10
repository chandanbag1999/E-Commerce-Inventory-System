using EIVMS.Application.Modules.Payments.Interfaces;

namespace EIVMS.Infrastructure.Services.Payments;

public class InMemoryWebhookIdempotencyStore : IWebhookIdempotencyStore
{
    private readonly Dictionary<string, DateTime> _store = new();
    private readonly object _lock = new();

    public async Task<bool> HasBeenProcessedAsync(string key, CancellationToken ct = default)
    {
        lock (_lock)
        {
            if (_store.TryGetValue(key, out var expiresAt))
            {
                if (DateTime.UtcNow < expiresAt)
                    return true;

                _store.Remove(key);
            }
            return false;
        }
    }

    public Task MarkAsProcessedAsync(string key, TimeSpan ttl, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _store[key] = DateTime.UtcNow.Add(ttl);
        }
        return Task.CompletedTask;
    }
}
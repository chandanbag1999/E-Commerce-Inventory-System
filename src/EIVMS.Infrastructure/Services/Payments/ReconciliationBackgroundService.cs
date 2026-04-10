using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using EIVMS.Application.Modules.Payments.Interfaces;
using EIVMS.Infrastructure.Services.Payments;

namespace EIVMS.Infrastructure.Services.Payments;

public class ReconciliationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReconciliationBackgroundService> _logger;
    private readonly PaymentSettings _settings;

    public ReconciliationBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<ReconciliationBackgroundService> logger,
        IOptions<PaymentSettings> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _settings = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Reconciliation Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                var nextRun = GetNextRunTime(now);
                var delay = nextRun - now;

                _logger.LogInformation(
                    "Next reconciliation scheduled at {NextRun} UTC (in {Minutes} minutes)",
                    nextRun, (int)delay.TotalMinutes);

                await Task.Delay(delay, stoppingToken);

                await RunReconciliationAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Reconciliation service stopping...");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reconciliation job failed. Will retry next cycle.");
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }
    }

    private async Task RunReconciliationAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running daily reconciliation job...");

        using var scope = _scopeFactory.CreateScope();
        var reconciliationService = scope.ServiceProvider
            .GetRequiredService<IReconciliationService>();

        var yesterday = DateTime.UtcNow.AddDays(-1).Date;
        var today = DateTime.UtcNow;

        try
        {
            _logger.LogInformation(
                "Reconciling for date {Date}", yesterday.ToString("yyyy-MM-dd"));

            var result = await reconciliationService.RunReconciliationAsync(
                yesterday, today);

            if (result.Success && result.Data is not null)
            {
                var report = result.Data;

                _logger.LogInformation(
                    "Reconciliation complete. " +
                    "Matched={Matched}, Mismatches={Mismatches}, " +
                    "MissingInDb={MissingInDb}, MissingInGateway={MissingInGateway}",
                    report.MatchedCount,
                    report.MismatchCount,
                    report.MissingInDb,
                    report.MissingInGateway);

                if (report.MismatchCount > 0)
                {
                    _logger.LogWarning(
                        "ALERT: {Count} payment mismatches detected!",
                        report.MismatchCount);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reconciliation failed");
        }
    }

    private static DateTime GetNextRunTime(DateTime now)
    {
        var todayAt2Am = now.Date.AddHours(2);
        return now < todayAt2Am
            ? todayAt2Am
            : todayAt2Am.AddDays(1);
    }
}
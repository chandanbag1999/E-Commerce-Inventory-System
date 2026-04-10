using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.Payments.DTOs;
using EIVMS.Application.Modules.Payments.Interfaces;
using Microsoft.Extensions.Logging;

namespace EIVMS.Application.Modules.Payments.Services;

public class ReconciliationService : IReconciliationService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentGatewayFactory _gatewayFactory;
    private readonly ILogger<ReconciliationService> _logger;

    private const string MismatchStatusMismatch = "STATUS_MISMATCH";
    private const string MismatchMissingInDb = "MISSING_IN_DB";
    private const string MismatchMissingInGateway = "MISSING_IN_GATEWAY";

    public ReconciliationService(
        IPaymentRepository paymentRepository,
        IPaymentGatewayFactory gatewayFactory,
        ILogger<ReconciliationService> logger)
    {
        _paymentRepository = paymentRepository;
        _gatewayFactory = gatewayFactory;
        _logger = logger;
    }

    public async Task<ApiResponse<ReconciliationReportDto>> RunReconciliationAsync(DateTime fromDate, DateTime toDate)
    {
        _logger.LogInformation("Starting reconciliation from {FromDate} to {ToDate}", fromDate, toDate);

        var (dbPayments, totalDbCount) = await _paymentRepository.GetByDateRangeAsync(fromDate, toDate, 1, int.MaxValue);

        var allGatewayPayments = new List<GatewayPaymentDetail>();
        var providers = new[] { Domain.Enums.Payments.PaymentProvider.Razorpay, Domain.Enums.Payments.PaymentProvider.Stripe };

        foreach (var provider in providers)
        {
            var gateway = _gatewayFactory.GetGateway(provider);
            if (gateway == null) continue;

            try
            {
                var gatewayPayments = await gateway.FetchPaymentsByDateRangeAsync(fromDate, toDate);
                allGatewayPayments.AddRange(gatewayPayments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch payments from gateway: {Provider}", provider);
            }
        }

        var mismatches = new List<ReconciliationMismatchDto>();
        var matchedIds = new HashSet<string>();

        foreach (var dbPayment in dbPayments)
        {
            var gatewayPayment = allGatewayPayments.FirstOrDefault(g =>
                g.ProviderPaymentId == dbPayment.ProviderPaymentId);

            if (gatewayPayment == null)
            {
                mismatches.Add(new ReconciliationMismatchDto(
                    PaymentId: dbPayment.Id,
                    DbStatus: dbPayment.Status.ToString(),
                    GatewayStatus: "NOT_FOUND",
                    Amount: dbPayment.Amount,
                    MismatchType: MismatchMissingInGateway,
                    DetectedAt: DateTime.UtcNow
                ));
                continue;
            }

            matchedIds.Add(gatewayPayment.ProviderPaymentId);

            var dbStatus = MapToGatewayStatus(dbPayment.Status);
            if (!string.Equals(dbStatus, gatewayPayment.Status, StringComparison.OrdinalIgnoreCase))
            {
                mismatches.Add(new ReconciliationMismatchDto(
                    PaymentId: dbPayment.Id,
                    DbStatus: dbPayment.Status.ToString(),
                    GatewayStatus: gatewayPayment.Status,
                    Amount: dbPayment.Amount,
                    MismatchType: MismatchStatusMismatch,
                    DetectedAt: DateTime.UtcNow
                ));
            }
        }

        var gatewayPaymentIds = allGatewayPayments.Select(g => g.ProviderPaymentId).ToHashSet();
        foreach (var dbPayment in dbPayments)
        {
            if (dbPayment.ProviderPaymentId != null)
            {
                gatewayPaymentIds.Remove(dbPayment.ProviderPaymentId);
            }
        }

        foreach (var missingProviderPaymentId in gatewayPaymentIds)
        {
            var gatewayPayment = allGatewayPayments.First(g => g.ProviderPaymentId == missingProviderPaymentId);
            mismatches.Add(new ReconciliationMismatchDto(
                PaymentId: Guid.Empty,
                DbStatus: "NOT_FOUND",
                GatewayStatus: gatewayPayment.Status,
                Amount: gatewayPayment.Amount,
                MismatchType: MismatchMissingInDb,
                DetectedAt: DateTime.UtcNow
            ));
        }

        var matchedCount = matchedIds.Count;
        var report = new ReconciliationReportDto(
            ReportDate: DateTime.UtcNow,
            TotalDbRecords: dbPayments.Count,
            TotalGatewayRecords: allGatewayPayments.Count,
            MatchedCount: matchedCount,
            MismatchCount: mismatches.Count,
            MissingInDb: mismatches.Count(m => m.MismatchType == MismatchMissingInDb),
            MissingInGateway: mismatches.Count(m => m.MismatchType == MismatchMissingInGateway),
            Mismatches: mismatches
        );

        _logger.LogInformation(
            "Reconciliation completed. DB: {DbCount}, Gateway: {GatewayCount}, Matched: {Matched}, Mismatches: {MismatchCount}",
            report.TotalDbRecords, report.TotalGatewayRecords, matchedCount, report.MismatchCount);

        return ApiResponse<ReconciliationReportDto>.SuccessResponse(report, "Reconciliation completed");
    }

    private static string MapToGatewayStatus(Domain.Enums.Payments.PaymentStatus status) => status switch
    {
        Domain.Enums.Payments.PaymentStatus.Success => "captured",
        Domain.Enums.Payments.PaymentStatus.Pending => "pending",
        Domain.Enums.Payments.PaymentStatus.Failed => "failed",
        Domain.Enums.Payments.PaymentStatus.Refunded => "refunded",
        Domain.Enums.Payments.PaymentStatus.PartiallyRefunded => "partially_refunded",
        _ => "unknown"
    };
}

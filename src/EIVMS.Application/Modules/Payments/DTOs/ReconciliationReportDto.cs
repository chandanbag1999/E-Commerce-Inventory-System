namespace EIVMS.Application.Modules.Payments.DTOs;

public record ReconciliationReportDto(
    DateTime ReportDate,
    int TotalDbRecords,
    int TotalGatewayRecords,
    int MatchedCount,
    int MismatchCount,
    int MissingInDb,
    int MissingInGateway,
    List<ReconciliationMismatchDto> Mismatches
);
using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.Payments.DTOs;

namespace EIVMS.Application.Modules.Payments.Interfaces;

public interface IReconciliationService
{
    Task<ApiResponse<ReconciliationReportDto>> RunReconciliationAsync(DateTime fromDate, DateTime toDate);
}

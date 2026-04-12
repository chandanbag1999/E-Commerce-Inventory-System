using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.Analytics.DTOs;

namespace EIVMS.Application.Modules.Analytics.Interfaces;

public interface IAnalyticsService
{
    Task<ApiResponse<DashboardAnalyticsDto>> GetDashboardAsync(Guid userId);
}

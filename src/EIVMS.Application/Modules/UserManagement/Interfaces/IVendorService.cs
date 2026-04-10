using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.UserManagement.DTOs.Vendor;

namespace EIVMS.Application.Modules.UserManagement.Interfaces;

public interface IVendorService
{
    Task<ApiResponse<VendorApplicationResponseDto>> ApplyForVendorAsync(CreateVendorApplicationDto dto, Guid userId);
    Task<ApiResponse<VendorApplicationResponseDto>> GetMyApplicationAsync(Guid userId);
    Task<ApiResponse<bool>> UpdateBankDetailsAsync(Guid applicationId, UpdateBankDetailsDto dto, Guid userId);
    Task<ApiResponse<bool>> SubmitApplicationAsync(Guid applicationId, Guid userId);
    Task<ApiResponse<List<VendorApplicationResponseDto>>> GetAllApplicationsAsync(string? status = null);
    Task<ApiResponse<VendorApplicationResponseDto>> GetApplicationByIdAsync(Guid applicationId);
    Task<ApiResponse<bool>> ReviewApplicationAsync(Guid applicationId, ReviewVendorApplicationDto dto, Guid adminUserId);
}
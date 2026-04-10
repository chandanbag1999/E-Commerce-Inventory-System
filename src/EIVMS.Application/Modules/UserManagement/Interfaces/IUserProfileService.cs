using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.UserManagement.DTOs.Address;
using EIVMS.Application.Modules.UserManagement.DTOs.User;
using Microsoft.AspNetCore.Http;

namespace EIVMS.Application.Modules.UserManagement.Interfaces;

public interface IUserProfileService
{
    Task<ApiResponse<UserProfileResponseDto>> GetProfileAsync(Guid userId);
    Task<ApiResponse<UserProfileResponseDto>> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
    Task<ApiResponse<string>> UploadProfilePictureAsync(Guid userId, IFormFile file);
    Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
    Task<ApiResponse<bool>> UpdateNotificationPreferencesAsync(Guid userId, UpdateNotificationPreferencesDto dto);
    Task<ApiResponse<PaginatedResponseDto<UserListResponseDto>>> GetUsersListAsync(int pageNumber, int pageSize, string? search = null, string? status = null);
    Task<ApiResponse<bool>> SuspendUserAsync(Guid userId, string reason, Guid adminUserId);
    Task<ApiResponse<bool>> ActivateUserAsync(Guid userId, Guid adminUserId);
    Task<ApiResponse<bool>> DeleteUserAsync(Guid userId, Guid adminUserId);
    Task<ApiResponse<bool>> RequestDataExportAsync(Guid userId);
    Task<ApiResponse<bool>> RequestDataDeletionAsync(Guid userId);
}
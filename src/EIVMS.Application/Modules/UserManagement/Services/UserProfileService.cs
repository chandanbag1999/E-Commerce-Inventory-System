using EIVMS.Application.Common.Interfaces;
using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.UserManagement.DTOs.User;
using EIVMS.Application.Modules.UserManagement.Interfaces;
using EIVMS.Domain.Entities.UserManagement;
using EIVMS.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace EIVMS.Application.Modules.UserManagement.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserManagementRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IFileStorageService _fileStorageService;

    public UserProfileService(
        IUserManagementRepository repository,
        IPasswordHasher passwordHasher,
        IFileStorageService fileStorageService)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _fileStorageService = fileStorageService;
    }

    public async Task<ApiResponse<UserProfileResponseDto>> GetProfileAsync(Guid userId)
    {
        var user = await _repository.GetUserWithProfileAsync(userId);
        if (user == null)
            return ApiResponse<UserProfileResponseDto>.ErrorResponse("User not found", 404);

        var orgUsers = await _repository.GetUserOrganizationsAsync(userId);
        var dto = MapToProfileDto(user, orgUsers);
        return ApiResponse<UserProfileResponseDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<UserProfileResponseDto>> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        var user = await _repository.GetUserWithProfileAsync(userId);
        if (user == null)
            return ApiResponse<UserProfileResponseDto>.ErrorResponse("User not found", 404);

        user.UpdateName(dto.FirstName, dto.LastName);

        var profile = await _repository.GetProfileByUserIdAsync(userId);
        if (profile == null)
        {
            profile = UserProfile.Create(userId);
            await _repository.CreateProfileAsync(profile);
        }

        profile.UpdatePreferences(dto.Language, dto.Currency, dto.TimeZone);
        profile.UpdateBio(dto.Bio, dto.DisplayName);

        if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            profile.UpdatePhone(dto.PhoneNumber);

        await _repository.UpdateProfileAsync(profile);

        await _repository.AddUserAuditLogAsync(
            UserAuditLog.Create(userId, "PROFILE_UPDATE", description: "User updated their profile"));

        var updatedUser = await _repository.GetUserWithProfileAsync(userId);
        var orgUsers = await _repository.GetUserOrganizationsAsync(userId);
        var responseDto = MapToProfileDto(updatedUser!, orgUsers);

        return ApiResponse<UserProfileResponseDto>.SuccessResponse(responseDto, "Profile updated successfully");
    }

    public async Task<ApiResponse<string>> UploadProfilePictureAsync(Guid userId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return ApiResponse<string>.ErrorResponse("No file provided");

        if (file.Length > 5 * 1024 * 1024)
            return ApiResponse<string>.ErrorResponse("File size cannot exceed 5MB");

        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return ApiResponse<string>.ErrorResponse("Only JPEG, PNG, and WebP images are allowed");

        var fileName = $"profiles/{userId}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var cdnUrl = await _fileStorageService.UploadAsync(file, fileName);

        var profile = await _repository.GetProfileByUserIdAsync(userId);
        if (profile == null)
        {
            profile = UserProfile.Create(userId);
            await _repository.CreateProfileAsync(profile);
        }

        profile.UpdateProfilePicture(cdnUrl);
        await _repository.UpdateProfileAsync(profile);

        return ApiResponse<string>.SuccessResponse(cdnUrl, "Profile picture uploaded successfully");
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmNewPassword)
            return ApiResponse<bool>.ErrorResponse("New passwords do not match");

        var user = await _repository.GetUserWithProfileAsync(userId);
        if (user == null)
            return ApiResponse<bool>.ErrorResponse("User not found", 404);

        if (!_passwordHasher.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
        {
            await _repository.AddUserAuditLogAsync(
                UserAuditLog.Create(userId, "PASSWORD_CHANGE_FAILED", isSuccessful: false, failureReason: "Current password incorrect"));

            return ApiResponse<bool>.ErrorResponse("Current password is incorrect", 401);
        }

        var newHash = _passwordHasher.HashPassword(dto.NewPassword);
        user.ChangePassword(newHash);

        await _repository.AddUserAuditLogAsync(
            UserAuditLog.Create(userId, "PASSWORD_CHANGED", description: "Password changed successfully"));

        return ApiResponse<bool>.SuccessResponse(true, "Password changed successfully");
    }

    public async Task<ApiResponse<bool>> UpdateNotificationPreferencesAsync(Guid userId, UpdateNotificationPreferencesDto dto)
    {
        var profile = await _repository.GetProfileByUserIdAsync(userId);
        if (profile == null)
        {
            profile = UserProfile.Create(userId);
            await _repository.CreateProfileAsync(profile);
        }

        profile.UpdateNotificationPreferences(dto.ToNotificationPreference());
        await _repository.UpdateProfileAsync(profile);

        return ApiResponse<bool>.SuccessResponse(true, "Notification preferences updated");
    }

    public async Task<ApiResponse<PaginatedResponseDto<UserListResponseDto>>> GetUsersListAsync(int pageNumber, int pageSize, string? search = null, string? status = null)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        UserStatus? userStatus = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<UserStatus>(status, true, out var parsedStatus))
            userStatus = parsedStatus;

        var (users, totalCount) = await _repository.GetUsersPagedAsync(pageNumber, pageSize, search, userStatus);

        var userDtos = users.Select(u => new UserListResponseDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            Status = u.Status,
            Roles = u.UserRoles.Select(ur => ur.Role?.Name ?? "").ToList(),
            CreatedAt = u.CreatedAt,
            LastLoginAt = u.LastLoginAt
        }).ToList();

        var result = new PaginatedResponseDto<UserListResponseDto>
        {
            Items = userDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return ApiResponse<PaginatedResponseDto<UserListResponseDto>>.SuccessResponse(result);
    }

    public async Task<ApiResponse<bool>> SuspendUserAsync(Guid userId, string reason, Guid adminUserId)
    {
        var user = await _repository.GetUserWithProfileAsync(userId);
        if (user == null)
            return ApiResponse<bool>.ErrorResponse("User not found", 404);

        if (user.Status == UserStatus.Suspended)
            return ApiResponse<bool>.ErrorResponse("User is already suspended");

        user.Suspend();

        await _repository.AddUserAuditLogAsync(
            UserAuditLog.Create(userId: userId, action: "USER_SUSPENDED", description: $"Suspended by admin: {adminUserId}. Reason: {reason}"));

        return ApiResponse<bool>.SuccessResponse(true, "User suspended successfully");
    }

    public async Task<ApiResponse<bool>> ActivateUserAsync(Guid userId, Guid adminUserId)
    {
        var user = await _repository.GetUserWithProfileAsync(userId);
        if (user == null)
            return ApiResponse<bool>.ErrorResponse("User not found", 404);

        user.Activate();

        await _repository.AddUserAuditLogAsync(
            UserAuditLog.Create(userId: userId, action: "USER_ACTIVATED", description: $"Activated by admin: {adminUserId}"));

        return ApiResponse<bool>.SuccessResponse(true, "User activated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteUserAsync(Guid userId, Guid adminUserId)
    {
        var user = await _repository.GetUserWithProfileAsync(userId);
        if (user == null)
            return ApiResponse<bool>.ErrorResponse("User not found", 404);

        user.SoftDelete();

        var profile = await _repository.GetProfileByUserIdAsync(userId);
        profile?.RequestDataDeletion();
        if (profile != null)
            await _repository.UpdateProfileAsync(profile);

        await _repository.AddUserAuditLogAsync(
            UserAuditLog.Create(userId: userId, action: "USER_DELETED", description: $"Soft deleted by admin: {adminUserId}"));

        return ApiResponse<bool>.SuccessResponse(true, "User deleted successfully");
    }

    public async Task<ApiResponse<bool>> RequestDataExportAsync(Guid userId)
    {
        var profile = await _repository.GetProfileByUserIdAsync(userId);
        if (profile == null)
            return ApiResponse<bool>.ErrorResponse("Profile not found", 404);

        profile.RequestDataExport();
        await _repository.UpdateProfileAsync(profile);

        return ApiResponse<bool>.SuccessResponse(true, "Data export request submitted. You will receive an email within 30 days.");
    }

    public async Task<ApiResponse<bool>> RequestDataDeletionAsync(Guid userId)
    {
        var profile = await _repository.GetProfileByUserIdAsync(userId);
        if (profile == null)
            return ApiResponse<bool>.ErrorResponse("Profile not found", 404);

        profile.RequestDataDeletion();
        await _repository.UpdateProfileAsync(profile);

        return ApiResponse<bool>.SuccessResponse(true, "Data deletion request submitted. Account will be deleted within 30 days.");
    }

    private static UserProfileResponseDto MapToProfileDto(Domain.Entities.Identity.User user, List<OrganizationUser> orgUsers)
    {
        var profile = user.UserProfile;

        return new UserProfileResponseDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Email = user.Email,
            Status = user.Status,
            IsEmailVerified = user.IsEmailVerified,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            PhoneNumber = profile?.PhoneNumber,
            IsPhoneVerified = profile?.IsPhoneVerified ?? false,
            ProfilePictureUrl = profile?.ProfilePictureUrl,
            Bio = profile?.Bio,
            DisplayName = profile?.DisplayName ?? user.FullName,
            Language = profile?.Language ?? "en",
            Currency = profile?.Currency ?? "INR",
            TimeZone = profile?.TimeZone ?? "Asia/Kolkata",
            NotificationPreferences = profile?.NotificationPreferences ?? NotificationPreference.Email,
            IsTwoFactorEnabled = profile?.IsTwoFactorEnabled ?? false,
            Roles = user.UserRoles.Select(ur => ur.Role?.Name ?? "").Where(r => !string.IsNullOrEmpty(r)).ToList(),
            Permissions = user.UserRoles.SelectMany(ur => ur.Role?.RolePermissions ?? new List<Domain.Entities.Identity.RolePermission>()).Select(rp => rp.Permission?.Name ?? "").Where(p => !string.IsNullOrEmpty(p)).Distinct().ToList(),
            Organizations = orgUsers.Select(ou => new UserOrganizationDto
            {
                OrganizationId = ou.OrganizationId,
                OrganizationName = ou.Organization?.Name ?? "",
                Designation = ou.Designation,
                Department = ou.Department,
                IsAdmin = ou.IsOrganizationAdmin
            }).ToList()
        };
    }
}
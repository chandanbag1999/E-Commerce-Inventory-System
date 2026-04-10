using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.UserManagement.DTOs.Organization;

namespace EIVMS.Application.Modules.UserManagement.Interfaces;

public interface IOrganizationService
{
    Task<ApiResponse<OrganizationResponseDto>> CreateOrganizationAsync(CreateOrganizationDto dto, Guid createdByUserId);
    Task<ApiResponse<OrganizationResponseDto>> GetOrganizationAsync(Guid organizationId);
    Task<ApiResponse<bool>> AddUserToOrganizationAsync(Guid organizationId, Guid userId, string? designation, string? department);
    Task<ApiResponse<bool>> RemoveUserFromOrganizationAsync(Guid organizationId, Guid userId, Guid adminUserId);
}
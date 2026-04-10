using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.UserManagement.DTOs.Organization;
using EIVMS.Application.Modules.UserManagement.Interfaces;
using EIVMS.Domain.Entities.UserManagement;
using EIVMS.Domain.Enums;

namespace EIVMS.Application.Modules.UserManagement.Services;

public class OrganizationService : IOrganizationService
{
    private readonly IUserManagementRepository _repository;

    public OrganizationService(IUserManagementRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<OrganizationResponseDto>> CreateOrganizationAsync(CreateOrganizationDto dto, Guid createdByUserId)
    {
        var organization = Organization.Create(dto.Name, dto.Type, createdByUserId, dto.Description);

        if (!string.IsNullOrWhiteSpace(dto.ContactEmail))
            organization.UpdateBusinessInfo(null, null, dto.WebsiteUrl, dto.ContactEmail, dto.ContactPhone);

        var created = await _repository.CreateOrganizationAsync(organization);

        var orgUser = OrganizationUser.Create(created.Id, createdByUserId, null, null, true);
        await _repository.AddUserToOrganizationAsync(orgUser);

        return ApiResponse<OrganizationResponseDto>.SuccessResponse(MapToDto(created), "Organization created successfully", 201);
    }

    public async Task<ApiResponse<OrganizationResponseDto>> GetOrganizationAsync(Guid organizationId)
    {
        var organization = await _repository.GetOrganizationByIdAsync(organizationId);
        if (organization == null)
            return ApiResponse<OrganizationResponseDto>.ErrorResponse("Organization not found", 404);

        return ApiResponse<OrganizationResponseDto>.SuccessResponse(MapToDto(organization));
    }

    public async Task<ApiResponse<bool>> AddUserToOrganizationAsync(Guid organizationId, Guid userId, string? designation, string? department)
    {
        var existing = await _repository.GetOrganizationUserAsync(organizationId, userId);
        if (existing != null)
            return ApiResponse<bool>.ErrorResponse("User already in organization", 409);

        var orgUser = OrganizationUser.Create(organizationId, userId, designation, department);
        await _repository.AddUserToOrganizationAsync(orgUser);

        return ApiResponse<bool>.SuccessResponse(true, "User added to organization");
    }

    public async Task<ApiResponse<bool>> RemoveUserFromOrganizationAsync(Guid organizationId, Guid userId, Guid adminUserId)
    {
        var orgUser = await _repository.GetOrganizationUserAsync(organizationId, userId);
        if (orgUser == null)
            return ApiResponse<bool>.ErrorResponse("User not in organization", 404);

        orgUser.Deactivate();
        return ApiResponse<bool>.SuccessResponse(true, "User removed from organization");
    }

    private static OrganizationResponseDto MapToDto(Organization org)
    {
        return new OrganizationResponseDto
        {
            Id = org.Id,
            Name = org.Name,
            Description = org.Description,
            Type = org.Type,
            Status = org.Status,
            GstNumber = org.GstNumber,
            WebsiteUrl = org.WebsiteUrl,
            LogoUrl = org.LogoUrl,
            ContactEmail = org.ContactEmail,
            ContactPhone = org.ContactPhone,
            Country = org.Country,
            PrimaryColor = org.PrimaryColor,
            MemberCount = org.OrganizationUsers.Count(o => o.IsActive),
            CreatedAt = org.CreatedAt
        };
    }
}
using EIVMS.Domain.Entities.UserManagement;
using EIVMS.Domain.Enums;

namespace EIVMS.Application.Modules.UserManagement.Interfaces;

public interface IUserManagementRepository
{
    Task<UserProfile?> GetProfileByUserIdAsync(Guid userId);
    Task<UserProfile> CreateProfileAsync(UserProfile profile);
    Task<UserProfile> UpdateProfileAsync(UserProfile profile);

    Task<List<Address>> GetUserAddressesAsync(Guid userId);
    Task<Address?> GetAddressByIdAsync(Guid addressId);
    Task<Address> AddAddressAsync(Address address);
    Task<Address> UpdateAddressAsync(Address address);
    Task UnsetAllDefaultAddressesAsync(Guid userId);

    Task<Organization?> GetOrganizationByIdAsync(Guid organizationId);
    Task<Organization> CreateOrganizationAsync(Organization organization);
    Task<Organization> UpdateOrganizationAsync(Organization organization);
    Task<OrganizationUser?> GetOrganizationUserAsync(Guid organizationId, Guid userId);
    Task AddUserToOrganizationAsync(OrganizationUser orgUser);
    Task<List<OrganizationUser>> GetUserOrganizationsAsync(Guid userId);

    Task<VendorApplication?> GetVendorApplicationByUserIdAsync(Guid userId);
    Task<VendorApplication?> GetVendorApplicationByIdAsync(Guid applicationId);
    Task<List<VendorApplication>> GetAllVendorApplicationsAsync(VendorOnboardingStatus? status);
    Task<VendorApplication> CreateVendorApplicationAsync(VendorApplication application);
    Task<VendorApplication> UpdateVendorApplicationAsync(VendorApplication application);
    Task AddVendorAuditLogAsync(VendorApplicationAuditLog auditLog);

    Task<(List<Domain.Entities.Identity.User> Users, int TotalCount)> GetUsersPagedAsync(
        int pageNumber, int pageSize, string? search, UserStatus? status);
    Task<Domain.Entities.Identity.User?> GetUserWithProfileAsync(Guid userId);

    Task AddUserAuditLogAsync(UserAuditLog auditLog);
}
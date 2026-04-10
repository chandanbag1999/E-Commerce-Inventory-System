using EIVMS.Application.Modules.UserManagement.Interfaces;
using EIVMS.Domain.Entities.Identity;
using EIVMS.Domain.Entities.UserManagement;
using EIVMS.Domain.Enums;
using EIVMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EIVMS.Infrastructure.Repositories.UserManagement;

public class UserManagementRepository : IUserManagementRepository
{
    private readonly AppDbContext _context;

    public UserManagementRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfile?> GetProfileByUserIdAsync(Guid userId)
    {
        return await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<UserProfile> CreateProfileAsync(UserProfile profile)
    {
        await _context.UserProfiles.AddAsync(profile);
        await _context.SaveChangesAsync();
        return profile;
    }

    public async Task<UserProfile> UpdateProfileAsync(UserProfile profile)
    {
        _context.UserProfiles.Update(profile);
        await _context.SaveChangesAsync();
        return profile;
    }

    public async Task<List<Address>> GetUserAddressesAsync(Guid userId)
    {
        return await _context.Addresses.Where(a => a.UserId == userId).OrderByDescending(a => a.IsDefault).ThenByDescending(a => a.CreatedAt).ToListAsync();
    }

    public async Task<Address?> GetAddressByIdAsync(Guid addressId)
    {
        return await _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId);
    }

    public async Task<Address> AddAddressAsync(Address address)
    {
        await _context.Addresses.AddAsync(address);
        await _context.SaveChangesAsync();
        return address;
    }

    public async Task<Address> UpdateAddressAsync(Address address)
    {
        _context.Addresses.Update(address);
        await _context.SaveChangesAsync();
        return address;
    }

    public async Task UnsetAllDefaultAddressesAsync(Guid userId)
    {
        var defaultAddresses = await _context.Addresses.Where(a => a.UserId == userId && a.IsDefault && !a.IsDeleted).ToListAsync();
        foreach (var address in defaultAddresses)
            address.UnsetDefault();
        await _context.SaveChangesAsync();
    }

    public async Task<Organization?> GetOrganizationByIdAsync(Guid organizationId)
    {
        return await _context.Organizations.Include(o => o.OrganizationUsers).ThenInclude(ou => ou.User).FirstOrDefaultAsync(o => o.Id == organizationId && !o.IsDeleted);
    }

    public async Task<Organization> CreateOrganizationAsync(Organization organization)
    {
        await _context.Organizations.AddAsync(organization);
        await _context.SaveChangesAsync();
        return organization;
    }

    public async Task<Organization> UpdateOrganizationAsync(Organization organization)
    {
        _context.Organizations.Update(organization);
        await _context.SaveChangesAsync();
        return organization;
    }

    public async Task<OrganizationUser?> GetOrganizationUserAsync(Guid organizationId, Guid userId)
    {
        return await _context.OrganizationUsers.FirstOrDefaultAsync(ou => ou.OrganizationId == organizationId && ou.UserId == userId && ou.IsActive);
    }

    public async Task AddUserToOrganizationAsync(OrganizationUser orgUser)
    {
        await _context.OrganizationUsers.AddAsync(orgUser);
        await _context.SaveChangesAsync();
    }

    public async Task<List<OrganizationUser>> GetUserOrganizationsAsync(Guid userId)
    {
        return await _context.OrganizationUsers.Include(ou => ou.Organization).Where(ou => ou.UserId == userId && ou.IsActive).ToListAsync();
    }

    public async Task<VendorApplication?> GetVendorApplicationByUserIdAsync(Guid userId)
    {
        return await _context.VendorApplications.Include(v => v.AuditLogs).FirstOrDefaultAsync(v => v.ApplicantUserId == userId);
    }

    public async Task<VendorApplication?> GetVendorApplicationByIdAsync(Guid applicationId)
    {
        return await _context.VendorApplications.Include(v => v.AuditLogs).Include(v => v.Organization).Include(v => v.ApplicantUser).FirstOrDefaultAsync(v => v.Id == applicationId);
    }

    public async Task<List<VendorApplication>> GetAllVendorApplicationsAsync(VendorOnboardingStatus? status)
    {
        var query = _context.VendorApplications.Include(v => v.Organization).Include(v => v.ApplicantUser).Include(v => v.AuditLogs).AsQueryable();
        if (status.HasValue)
            query = query.Where(v => v.Status == status.Value);
        return await query.OrderByDescending(v => v.CreatedAt).ToListAsync();
    }

    public async Task<VendorApplication> CreateVendorApplicationAsync(VendorApplication application)
    {
        await _context.VendorApplications.AddAsync(application);
        await _context.SaveChangesAsync();
        return application;
    }

    public async Task<VendorApplication> UpdateVendorApplicationAsync(VendorApplication application)
    {
        _context.VendorApplications.Update(application);
        await _context.SaveChangesAsync();
        return application;
    }

    public async Task AddVendorAuditLogAsync(VendorApplicationAuditLog auditLog)
    {
        await _context.VendorApplicationAuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task<(List<User> Users, int TotalCount)> GetUsersPagedAsync(int pageNumber, int pageSize, string? search, UserStatus? status)
    {
        var query = _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).Where(u => u.Status != UserStatus.Deleted).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(u => u.FirstName.ToLower().Contains(search) || u.LastName.ToLower().Contains(search) || u.Email.ToLower().Contains(search));
        }

        if (status.HasValue)
            query = query.Where(u => u.Status == status.Value);

        var totalCount = await query.CountAsync();

        var users = await query.OrderByDescending(u => u.CreatedAt).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return (users, totalCount);
    }

    public async Task<User?> GetUserWithProfileAsync(Guid userId)
    {
        return await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission).Include(u => u.UserProfile).FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task AddUserAuditLogAsync(UserAuditLog auditLog)
    {
        await _context.UserAuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();
    }
}
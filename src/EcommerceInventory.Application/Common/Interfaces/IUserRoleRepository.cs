using EcommerceInventory.Domain.Entities;

namespace EcommerceInventory.Application.Common.Interfaces;

public interface IUserRoleRepository
{
    IQueryable<UserRole> Query();
    Task<UserRole?> FindAsync(Guid userId, Guid roleId, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid userId, Guid roleId, CancellationToken ct = default);
    Task AddAsync(UserRole userRole, CancellationToken ct = default);
    void Remove(UserRole userRole);
}

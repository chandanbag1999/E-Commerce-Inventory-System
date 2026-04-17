using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Infrastructure.Persistence.Repositories;

public class UserRoleRepository : IUserRoleRepository
{
    private readonly AppDbContext _context;
    private readonly DbSet<UserRole> _dbSet;

    public UserRoleRepository(AppDbContext context)
    {
        _context = context;
        _dbSet   = context.Set<UserRole>();
    }

    public IQueryable<UserRole> Query()
        => _dbSet.AsQueryable();

    public async Task<UserRole?> FindAsync(Guid userId, Guid roleId, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(
               ur => ur.UserId == userId && ur.RoleId == roleId, ct);

    public async Task<bool> ExistsAsync(Guid userId, Guid roleId, CancellationToken ct = default)
        => await _dbSet.AnyAsync(
               ur => ur.UserId == userId && ur.RoleId == roleId, ct);

    public async Task AddAsync(UserRole userRole, CancellationToken ct = default)
        => await _dbSet.AddAsync(userRole, ct);

    public void Remove(UserRole userRole)
        => _dbSet.Remove(userRole);
}

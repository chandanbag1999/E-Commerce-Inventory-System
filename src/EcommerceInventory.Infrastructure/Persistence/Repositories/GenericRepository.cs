using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Infrastructure.Persistence.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet   = context.Set<T>();
    }

    public IQueryable<T> Query()
        => _dbSet.AsQueryable();

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _dbSet.FindAsync(new object[] { id }, ct);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await _dbSet.ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await _dbSet.AddAsync(entity, ct);

    public void Update(T entity)
        => _dbSet.Update(entity);

    public void Remove(T entity)
        => _dbSet.Remove(entity);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => await _dbSet.AnyAsync(e => e.Id == id, ct);
}

using EcommerceInventory.Domain.Common;

namespace EcommerceInventory.Application.Common.Interfaces;

public interface IGenericRepository<T> where T : BaseEntity
{
    IQueryable<T> Query();
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
}

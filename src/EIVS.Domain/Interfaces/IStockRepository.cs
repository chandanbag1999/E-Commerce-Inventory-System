using EcommerceInventorySystem.Domain.Entities;

namespace EcommerceInventorySystem.Domain.Interfaces;

public interface IStockRepository : IGenericRepository<Stock>
{
    Task<Stock?> GetByProductIdAsync(int productId);
    Task<IEnumerable<Stock>> GetLowStockItemsAsync();
}
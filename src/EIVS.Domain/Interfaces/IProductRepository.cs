using EcommerceInventorySystem.Domain.Entities;

namespace EcommerceInventorySystem.Domain.Interfaces;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
    Task<Product?> GetBySkuAsync(string sku);
}
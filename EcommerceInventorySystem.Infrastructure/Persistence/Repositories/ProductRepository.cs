using EcommerceInventorySystem.Domain.Entities;
using EcommerceInventorySystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventorySystem.Infrastructure.Persistence.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId) =>
        await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();

    public async Task<Product?> GetBySkuAsync(string sku) =>
        await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.SKU == sku);
}
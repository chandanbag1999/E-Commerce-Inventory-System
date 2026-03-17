using EcommerceInventorySystem.Domain.Entities;
using EcommerceInventorySystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventorySystem.Infrastructure.Persistence.Repositories;

public class StockRepository : GenericRepository<Stock>, IStockRepository
{
    public StockRepository(AppDbContext context) : base(context) { }

    public async Task<Stock?> GetByProductIdAsync(int productId) =>
        await _context.Stocks
            .Include(s => s.Product)
            .FirstOrDefaultAsync(s => s.ProductId == productId);

    public async Task<IEnumerable<Stock>> GetLowStockItemsAsync() =>
        await _context.Stocks
            .Include(s => s.Product)
            .Where(s => s.Quantity <= s.LowStockThreshold)
            .ToListAsync();
}
using EcommerceInventorySystem.Domain.Interfaces;
using EcommerceInventorySystem.Infrastructure.Persistence;
using EcommerceInventorySystem.Infrastructure.Persistence.Repositories;

namespace EcommerceInventorySystem.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public IProductRepository Products { get; }
    public IStockRepository Stocks { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Products = new ProductRepository(context);
        Stocks = new StockRepository(context);
    }

    public async Task<int> SaveChangesAsync() => 
        await _context.SaveChangesAsync();

    public void Dispose() => 
        _context.Dispose();
}
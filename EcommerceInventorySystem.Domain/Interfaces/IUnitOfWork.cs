namespace EcommerceInventorySystem.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    IStockRepository Stocks { get; }
    Task<int> SaveChangesAsync();
}
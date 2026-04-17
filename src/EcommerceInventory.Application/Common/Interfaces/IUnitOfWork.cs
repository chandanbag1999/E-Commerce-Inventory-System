using EcommerceInventory.Domain.Entities;

namespace EcommerceInventory.Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<User>                   Users                   { get; }
    IGenericRepository<Role>                   Roles                   { get; }
    IGenericRepository<Permission>             Permissions             { get; }
    IGenericRepository<RefreshToken>           RefreshTokens           { get; }
    IGenericRepository<PasswordResetToken>     PasswordResetTokens     { get; }
    IGenericRepository<EmailVerificationToken> EmailVerificationTokens { get; }
    IGenericRepository<Category>               Categories              { get; }
    IGenericRepository<Product>                Products                { get; }
    IGenericRepository<ProductImage>           ProductImages           { get; }
    IGenericRepository<Warehouse>              Warehouses              { get; }
    IGenericRepository<Stock>                  Stocks                  { get; }
    IGenericRepository<StockMovement>          StockMovements          { get; }
    IGenericRepository<Supplier>               Suppliers               { get; }
    IGenericRepository<PurchaseOrder>          PurchaseOrders          { get; }
    IGenericRepository<PurchaseOrderItem>      PurchaseOrderItems      { get; }
    IGenericRepository<SalesOrder>             SalesOrders             { get; }
    IGenericRepository<SalesOrderItem>         SalesOrderItems         { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}

using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Infrastructure.Persistence.Repositories;

namespace EcommerceInventory.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public IGenericRepository<User>                   Users                   { get; }
    public IGenericRepository<Role>                   Roles                   { get; }
    public IGenericRepository<Permission>             Permissions             { get; }
    public IGenericRepository<RefreshToken>           RefreshTokens           { get; }
    public IGenericRepository<PasswordResetToken>     PasswordResetTokens     { get; }
    public IGenericRepository<EmailVerificationToken> EmailVerificationTokens { get; }
    public IGenericRepository<Category>               Categories              { get; }
    public IGenericRepository<Product>                Products                { get; }
    public IGenericRepository<ProductImage>           ProductImages           { get; }
    public IGenericRepository<Warehouse>              Warehouses              { get; }
    public IGenericRepository<Stock>                  Stocks                  { get; }
    public IGenericRepository<StockMovement>          StockMovements          { get; }
    public IGenericRepository<Supplier>               Suppliers               { get; }
    public IGenericRepository<PurchaseOrder>          PurchaseOrders          { get; }
    public IGenericRepository<PurchaseOrderItem>      PurchaseOrderItems      { get; }
    public IGenericRepository<SalesOrder>             SalesOrders             { get; }
    public IGenericRepository<SalesOrderItem>         SalesOrderItems         { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context                = context;
        Users                   = new GenericRepository<User>(context);
        Roles                   = new GenericRepository<Role>(context);
        Permissions             = new GenericRepository<Permission>(context);
        RefreshTokens           = new GenericRepository<RefreshToken>(context);
        PasswordResetTokens     = new GenericRepository<PasswordResetToken>(context);
        EmailVerificationTokens = new GenericRepository<EmailVerificationToken>(context);
        Categories              = new GenericRepository<Category>(context);
        Products                = new GenericRepository<Product>(context);
        ProductImages           = new GenericRepository<ProductImage>(context);
        Warehouses              = new GenericRepository<Warehouse>(context);
        Stocks                  = new GenericRepository<Stock>(context);
        StockMovements          = new GenericRepository<StockMovement>(context);
        Suppliers               = new GenericRepository<Supplier>(context);
        PurchaseOrders          = new GenericRepository<PurchaseOrder>(context);
        PurchaseOrderItems      = new GenericRepository<PurchaseOrderItem>(context);
        SalesOrders             = new GenericRepository<SalesOrder>(context);
        SalesOrderItems         = new GenericRepository<SalesOrderItem>(context);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => await _context.Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct = default)
        => await _context.Database.CommitTransactionAsync(ct);

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
        => await _context.Database.RollbackTransactionAsync(ct);

    public void Dispose() => _context.Dispose();
}

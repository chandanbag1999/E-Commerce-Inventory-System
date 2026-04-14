using EcommerceInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceInventory.Infrastructure.Persistence.Configurations;

public class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        builder.ToTable("stocks");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Quantity)
            .IsRequired();

        builder.Property(s => s.ReservedQty)
            .IsRequired();

        // Unique constraint: one stock entry per product per warehouse
        builder.HasIndex(s => new { s.ProductId, s.WarehouseId })
            .IsUnique();

        builder.HasOne(s => s.Product)
            .WithMany(p => p.Stocks)
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Warehouse)
            .WithMany(w => w.Stocks)
            .HasForeignKey(s => s.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.ProductId)
            .HasDatabaseName("idx_stocks_product");

        builder.HasIndex(s => s.WarehouseId)
            .HasDatabaseName("idx_stocks_warehouse");
    }
}

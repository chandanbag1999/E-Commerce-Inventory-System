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
        builder.Property(s => s.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(s => s.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(s => s.WarehouseId).HasColumnName("warehouse_id").IsRequired();
        builder.Property(s => s.Quantity).HasColumnName("quantity").HasDefaultValue(0).IsRequired();
        builder.Property(s => s.ReservedQty).HasColumnName("reserved_qty").HasDefaultValue(0).IsRequired();
        builder.Ignore(s => s.AvailableQty);
        builder.Property(s => s.LastCountedAt).HasColumnName("last_counted_at");
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()").IsRequired();

        builder.HasIndex(s => new { s.ProductId, s.WarehouseId }).IsUnique().HasDatabaseName("idx_stocks_product_warehouse");
        builder.HasIndex(s => s.ProductId).HasDatabaseName("idx_stocks_product");
        builder.HasIndex(s => s.WarehouseId).HasDatabaseName("idx_stocks_warehouse");

        builder.HasMany(s => s.StockMovements).WithOne(sm => sm.Stock).HasForeignKey(sm => sm.StockId).OnDelete(DeleteBehavior.Cascade);
    }
}
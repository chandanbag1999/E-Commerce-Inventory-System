using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceInventory.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(p => p.CategoryId).HasColumnName("category_id").IsRequired();
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(p => p.Slug).HasColumnName("slug").HasMaxLength(250).IsRequired();
        builder.HasIndex(p => p.Slug).IsUnique().HasDatabaseName("idx_products_slug");
        builder.Property(p => p.Description).HasColumnName("description").HasColumnType("text");
        builder.Property(p => p.Sku).HasColumnName("sku").HasMaxLength(100).IsRequired();
        builder.HasIndex(p => p.Sku).IsUnique().HasDatabaseName("idx_products_sku");
        builder.Property(p => p.Barcode).HasColumnName("barcode").HasMaxLength(100);
        builder.Property(p => p.UnitPrice).HasColumnName("unit_price").HasColumnType("numeric(18,2)").HasDefaultValue(0m).IsRequired();
        builder.Property(p => p.CostPrice).HasColumnName("cost_price").HasColumnType("numeric(18,2)").HasDefaultValue(0m).IsRequired();
        builder.Property(p => p.ReorderLevel).HasColumnName("reorder_level").HasDefaultValue(0).IsRequired();
        builder.Property(p => p.ReorderQty).HasColumnName("reorder_qty").HasDefaultValue(0).IsRequired();
        builder.Property(p => p.Status).HasColumnName("status").HasMaxLength(20).HasConversion<string>().HasDefaultValue(ProductStatus.Active).IsRequired();
        builder.Property(p => p.WeightKg).HasColumnName("weight_kg").HasColumnType("numeric(10,3)");
        builder.Property(p => p.CreatedBy).HasColumnName("created_by");
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(p => p.DeletedAt).HasColumnName("deleted_at");
        builder.HasQueryFilter(p => p.DeletedAt == null);

        builder.HasIndex(p => p.CategoryId).HasDatabaseName("idx_products_category");
        builder.HasIndex(p => p.Status).HasDatabaseName("idx_products_status");

        builder.HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(p => p.Images).WithOne(pi => pi.Product).HasForeignKey(pi => pi.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Stocks).WithOne(s => s.Product).HasForeignKey(s => s.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}
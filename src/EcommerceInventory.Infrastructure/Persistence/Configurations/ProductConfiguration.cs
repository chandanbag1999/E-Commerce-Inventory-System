using EcommerceInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceInventory.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Slug)
            .HasMaxLength(250)
            .IsRequired();

        builder.HasIndex(p => p.Slug)
            .IsUnique();

        builder.Property(p => p.Sku)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(p => p.Sku)
            .IsUnique();

        builder.Property(p => p.Barcode)
            .HasMaxLength(100);

        builder.Property(p => p.UnitPrice)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(p => p.CostPrice)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.WeightKg)
            .HasColumnType("numeric(10,3)");

        builder.Property(p => p.DeletedAt);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.CategoryId)
            .HasDatabaseName("idx_products_category");

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("idx_products_status")
            .HasFilter("\"DeletedAt\" IS NULL");
    }
}

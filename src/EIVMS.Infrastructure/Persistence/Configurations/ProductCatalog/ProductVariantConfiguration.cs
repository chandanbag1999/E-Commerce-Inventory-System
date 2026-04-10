using EIVMS.Domain.Entities.ProductCatalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EIVMS.Infrastructure.Persistence.Configurations.ProductCatalog;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");

        builder.HasKey(pv => pv.Id);

        builder.Property(pv => pv.SKU)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(pv => pv.SKU)
            .IsUnique()
            .HasDatabaseName("IX_ProductVariants_SKU");

        builder.Property(pv => pv.Barcode)
            .HasMaxLength(100);

        builder.Property(pv => pv.Name)
            .HasMaxLength(200);

        builder.Property(pv => pv.Price)
            .HasPrecision(18, 2);

        builder.Property(pv => pv.CompareAtPrice)
            .HasPrecision(18, 2);

        builder.Property(pv => pv.CostPrice)
            .HasPrecision(18, 2);

        builder.Property(pv => pv.WeightKg)
            .HasPrecision(10, 3);

        builder.Property(pv => pv.StockQuantity)
            .HasDefaultValue(0);

        builder.Property(pv => pv.ReservedQuantity)
            .HasDefaultValue(0);

        builder.Property(pv => pv.TrackInventory)
            .HasDefaultValue(true);

        builder.Property(pv => pv.AllowBackorder)
            .HasDefaultValue(false);

        builder.HasOne(pv => pv.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(pv => pv.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pv => pv.ProductId);
        builder.HasIndex(pv => pv.IsActive);
        builder.HasIndex(pv => pv.IsDefault);
    }
}
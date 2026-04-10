using EIVMS.Domain.Entities.ProductCatalog;
using EIVMS.Domain.Enums.ProductCatalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EIVMS.Infrastructure.Persistence.Configurations.ProductCatalog;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(p => p.Slug)
            .HasMaxLength(300)
            .IsRequired();

        builder.HasIndex(p => p.Slug)
            .IsUnique()
            .HasDatabaseName("IX_Products_Slug");

        builder.Property(p => p.SKU)
            .HasMaxLength(100);

        builder.HasIndex(p => p.SKU)
            .HasDatabaseName("IX_Products_SKU");

        builder.Property(p => p.Barcode)
            .HasMaxLength(100);

        builder.Property(p => p.Brand)
            .HasMaxLength(100);

        builder.Property(p => p.Model)
            .HasMaxLength(100);

        builder.Property(p => p.ShortDescription)
            .HasMaxLength(500);

        builder.Property(p => p.FullDescription)
            .HasMaxLength(10000);

        builder.Property(p => p.BasePrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.CompareAtPrice)
            .HasPrecision(18, 2);

        builder.Property(p => p.Currency)
            .HasMaxLength(10)
            .HasDefaultValue("INR");

        builder.Property(p => p.TaxRate)
            .HasPrecision(5, 2)
            .HasDefaultValue(0);

        builder.Property(p => p.HsnCode)
            .HasMaxLength(20);

        builder.Property(p => p.WeightKg)
            .HasPrecision(10, 3);

        builder.Property(p => p.LengthCm)
            .HasPrecision(10, 2);

        builder.Property(p => p.WidthCm)
            .HasPrecision(10, 2);

        builder.Property(p => p.HeightCm)
            .HasPrecision(10, 2);

        builder.Property(p => p.MetaTitle)
            .HasMaxLength(200);

        builder.Property(p => p.MetaDescription)
            .HasMaxLength(500);

        builder.Property(p => p.MetaKeywords)
            .HasMaxLength(500);

        builder.Property(p => p.Tags)
            .HasMaxLength(500);

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.PricingType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.IsFeatured);
        builder.HasIndex(p => p.IsDeleted);
        builder.HasIndex(p => p.CreatedAt);
    }
}
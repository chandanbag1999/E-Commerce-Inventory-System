using EIVMS.Domain.Entities.ProductCatalog;
using EIVMS.Domain.Enums.ProductCatalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EIVMS.Infrastructure.Persistence.Configurations.ProductCatalog;

public class ProductMediaConfiguration : IEntityTypeConfiguration<ProductMedia>
{
    public void Configure(EntityTypeBuilder<ProductMedia> builder)
    {
        builder.ToTable("ProductMedias");

        builder.HasKey(pm => pm.Id);

        builder.Property(pm => pm.Url)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(pm => pm.ThumbnailUrl)
            .HasMaxLength(500);

        builder.Property(pm => pm.AltText)
            .HasMaxLength(200);

        builder.Property(pm => pm.Title)
            .HasMaxLength(200);

        builder.Property(pm => pm.FileName)
            .HasMaxLength(260);

        builder.Property(pm => pm.MimeType)
            .HasMaxLength(100);

        builder.Property(pm => pm.VideoProvider)
            .HasMaxLength(50);

        builder.Property(pm => pm.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(pm => pm.Product)
            .WithMany(p => p.Media)
            .HasForeignKey(pm => pm.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pm => pm.ProductId);
        builder.HasIndex(pm => pm.IsPrimary);
        builder.HasIndex(pm => pm.IsDeleted);
    }
}
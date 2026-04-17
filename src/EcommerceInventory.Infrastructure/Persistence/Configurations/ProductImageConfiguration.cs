using EcommerceInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceInventory.Infrastructure.Persistence.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("product_images");
        builder.HasKey(pi => pi.Id);
        builder.Property(pi => pi.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(pi => pi.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(pi => pi.CloudinaryId).HasColumnName("cloudinary_id").HasMaxLength(200).IsRequired();
        builder.Property(pi => pi.ImageUrl).HasColumnName("image_url").HasMaxLength(500).IsRequired();
        builder.Property(pi => pi.IsPrimary).HasColumnName("is_primary").HasDefaultValue(false).IsRequired();
        builder.Property(pi => pi.DisplayOrder).HasColumnName("display_order").HasDefaultValue(0).IsRequired();
        builder.Property(pi => pi.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
        builder.Ignore(pi => pi.UpdatedAt);
        builder.HasIndex(pi => pi.ProductId).HasDatabaseName("idx_product_images_product");
    }
}
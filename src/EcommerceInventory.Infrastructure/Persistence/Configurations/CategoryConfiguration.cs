using EcommerceInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceInventory.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(c => c.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder.Property(c => c.Slug).HasColumnName("slug").HasMaxLength(200).IsRequired();
        builder.HasIndex(c => c.Slug).IsUnique().HasDatabaseName("idx_categories_slug");
        builder.Property(c => c.Description).HasColumnName("description").HasColumnType("text");
        builder.Property(c => c.ImageUrl).HasColumnName("image_url").HasMaxLength(500);
        builder.Property(c => c.CloudinaryId).HasColumnName("cloudinary_id").HasMaxLength(200);
        builder.Property(c => c.ParentId).HasColumnName("parent_id");
        builder.Property(c => c.IsActive).HasColumnName("is_active").HasDefaultValue(true).IsRequired();
        builder.Property(c => c.SortOrder).HasColumnName("sort_order").HasDefaultValue(0).IsRequired();
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(c => c.DeletedAt).HasColumnName("deleted_at");
        builder.HasQueryFilter(c => c.DeletedAt == null);

        builder.HasOne(c => c.Parent).WithMany(c => c.Children).HasForeignKey(c => c.ParentId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(c => c.ParentId).HasDatabaseName("idx_categories_parent");
    }
}
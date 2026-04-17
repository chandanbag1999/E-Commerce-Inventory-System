using EcommerceInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceInventory.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder.HasIndex(p => p.Name).IsUnique().HasDatabaseName("idx_permissions_name");
        builder.Property(p => p.Module).HasColumnName("module").HasMaxLength(100).IsRequired();
        builder.Property(p => p.Description).HasColumnName("description").HasMaxLength(300);
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
        builder.Ignore(p => p.UpdatedAt);

        builder.HasMany(p => p.RolePermissions).WithOne(rp => rp.Permission).HasForeignKey(rp => rp.PermissionId).OnDelete(DeleteBehavior.Cascade);
    }
}
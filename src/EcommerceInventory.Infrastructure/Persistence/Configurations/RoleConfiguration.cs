using EcommerceInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceInventory.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(r => r.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.HasIndex(r => r.Name).IsUnique().HasDatabaseName("idx_roles_name");
        builder.Property(r => r.Description).HasColumnName("description").HasMaxLength(300);
        builder.Property(r => r.HierarchyLevel).HasColumnName("hierarchy_level").HasDefaultValue(100).IsRequired();
        builder.Property(r => r.IsSystemRole).HasColumnName("is_system_role").HasDefaultValue(false).IsRequired();
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()").IsRequired();

        builder.HasMany(r => r.RolePermissions).WithOne(rp => rp.Role).HasForeignKey(rp => rp.RoleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(r => r.UserRoles).WithOne(ur => ur.Role).HasForeignKey(ur => ur.RoleId).OnDelete(DeleteBehavior.Cascade);
    }
}
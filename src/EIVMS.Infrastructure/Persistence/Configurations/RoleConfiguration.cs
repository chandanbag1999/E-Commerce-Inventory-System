using EIVMS.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EIVMS.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.NormalizedName)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(r => r.NormalizedName)
            .IsUnique()
            .HasDatabaseName("IX_Roles_NormalizedName");

        builder.Property(r => r.Description)
            .HasMaxLength(200);
    }
}
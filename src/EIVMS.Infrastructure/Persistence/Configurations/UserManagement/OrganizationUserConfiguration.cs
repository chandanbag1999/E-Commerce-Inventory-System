using EIVMS.Domain.Entities.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EIVMS.Infrastructure.Persistence.Configurations.UserManagement;

public class OrganizationUserConfiguration : IEntityTypeConfiguration<OrganizationUser>
{
    public void Configure(EntityTypeBuilder<OrganizationUser> builder)
    {
        builder.ToTable("OrganizationUsers");
        builder.HasKey(ou => ou.Id);

        builder.HasIndex(ou => new { ou.OrganizationId, ou.UserId }).IsUnique().HasDatabaseName("IX_OrganizationUsers_OrgId_UserId");

        builder.Property(ou => ou.Designation).HasMaxLength(100);
        builder.Property(ou => ou.Department).HasMaxLength(100);

        builder.HasOne(ou => ou.Organization).WithMany(o => o.OrganizationUsers).HasForeignKey(ou => ou.OrganizationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(ou => ou.User).WithMany().HasForeignKey(ou => ou.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
using EIVMS.Domain.Entities.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EIVMS.Infrastructure.Persistence.Configurations.UserManagement;

public class VendorApplicationAuditLogConfiguration : IEntityTypeConfiguration<VendorApplicationAuditLog>
{
    public void Configure(EntityTypeBuilder<VendorApplicationAuditLog> builder)
    {
        builder.ToTable("VendorApplicationAuditLogs");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Action).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Notes).HasMaxLength(500);
        builder.Property(a => a.IpAddress).HasMaxLength(45);
        builder.Property(a => a.FromStatus).HasConversion<int>();
        builder.Property(a => a.ToStatus).HasConversion<int>();

        builder.HasIndex(a => a.VendorApplicationId).HasDatabaseName("IX_VendorAuditLogs_ApplicationId");
    }
}
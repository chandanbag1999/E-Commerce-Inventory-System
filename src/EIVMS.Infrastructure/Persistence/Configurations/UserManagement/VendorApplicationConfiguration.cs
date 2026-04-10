using EIVMS.Domain.Entities.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EIVMS.Infrastructure.Persistence.Configurations.UserManagement;

public class VendorApplicationConfiguration : IEntityTypeConfiguration<VendorApplication>
{
    public void Configure(EntityTypeBuilder<VendorApplication> builder)
    {
        builder.ToTable("VendorApplications");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.BusinessName).HasMaxLength(200).IsRequired();
        builder.Property(v => v.BusinessType).HasMaxLength(100).IsRequired();
        builder.Property(v => v.BusinessDescription).HasMaxLength(1000);
        builder.Property(v => v.GstNumber).HasMaxLength(20);
        builder.Property(v => v.BusinessLicenseNumber).HasMaxLength(100);
        builder.Property(v => v.GstDocumentUrl).HasMaxLength(500);
        builder.Property(v => v.BusinessLicenseUrl).HasMaxLength(500);
        builder.Property(v => v.PanCardUrl).HasMaxLength(500);
        builder.Property(v => v.BankStatementUrl).HasMaxLength(500);
        builder.Property(v => v.BankAccountNumber).HasMaxLength(30);
        builder.Property(v => v.BankIfscCode).HasMaxLength(20);
        builder.Property(v => v.BankName).HasMaxLength(100);
        builder.Property(v => v.AccountHolderName).HasMaxLength(100);
        builder.Property(v => v.ReviewNotes).HasMaxLength(1000);
        builder.Property(v => v.RejectionReason).HasMaxLength(500);
        builder.Property(v => v.Status).HasConversion<int>();

        builder.HasIndex(v => v.Status).HasDatabaseName("IX_VendorApplications_Status");
        builder.HasIndex(v => v.ApplicantUserId).HasDatabaseName("IX_VendorApplications_ApplicantUserId");

        builder.HasOne(v => v.Organization).WithOne(o => o.VendorApplication).HasForeignKey<VendorApplication>(v => v.OrganizationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(v => v.ApplicantUser).WithMany().HasForeignKey(v => v.ApplicantUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(v => v.AuditLogs).WithOne(a => a.VendorApplication).HasForeignKey(a => a.VendorApplicationId).OnDelete(DeleteBehavior.Cascade);
    }
}
using EIVMS.Domain.Entities.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EIVMS.Infrastructure.Persistence.Configurations.UserManagement;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Name).HasMaxLength(200).IsRequired();
        builder.Property(o => o.Description).HasMaxLength(1000);
        builder.Property(o => o.GstNumber).HasMaxLength(20);
        builder.Property(o => o.PanNumber).HasMaxLength(20);
        builder.Property(o => o.WebsiteUrl).HasMaxLength(200);
        builder.Property(o => o.LogoUrl).HasMaxLength(500);
        builder.Property(o => o.ContactEmail).HasMaxLength(256);
        builder.Property(o => o.ContactPhone).HasMaxLength(20);
        builder.Property(o => o.Country).HasMaxLength(100);
        builder.Property(o => o.State).HasMaxLength(100);
        builder.Property(o => o.City).HasMaxLength(100);
        builder.Property(o => o.PrimaryColor).HasMaxLength(7);
        builder.Property(o => o.SecondaryColor).HasMaxLength(7);
        builder.Property(o => o.CustomDomain).HasMaxLength(200);
        builder.Property(o => o.Type).HasConversion<int>();
        builder.Property(o => o.Status).HasConversion<int>();

        builder.HasQueryFilter(o => !o.IsDeleted);
        builder.HasIndex(o => o.Name).HasDatabaseName("IX_Organizations_Name");
    }
}
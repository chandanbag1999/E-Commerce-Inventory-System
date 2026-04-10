using EIVMS.Domain.Entities.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EIVMS.Infrastructure.Persistence.Configurations.UserManagement;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");
        builder.HasKey(p => p.Id);

        builder.HasOne(p => p.User).WithOne(u => u.UserProfile).HasForeignKey<UserProfile>(p => p.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.UserId).IsUnique().HasDatabaseName("IX_UserProfiles_UserId");

        builder.Property(p => p.Language).HasMaxLength(10).HasDefaultValue("en");
        builder.Property(p => p.Currency).HasMaxLength(10).HasDefaultValue("INR");
        builder.Property(p => p.TimeZone).HasMaxLength(50).HasDefaultValue("Asia/Kolkata");
        builder.Property(p => p.ProfilePictureUrl).HasMaxLength(500);
        builder.Property(p => p.PhoneNumber).HasMaxLength(20);
        builder.Property(p => p.Bio).HasMaxLength(500);
        builder.Property(p => p.DisplayName).HasMaxLength(100);
        builder.Property(p => p.TwoFactorSecret).HasMaxLength(256);
        builder.Property(p => p.TwoFactorBackupCodes).HasMaxLength(1000);
        builder.Property(p => p.NotificationPreferences).HasConversion<int>();
    }
}
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceInventory.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(u => u.FullName).HasColumnName("full_name").HasMaxLength(150).IsRequired();
        builder.Property(u => u.Email).HasColumnName("email").HasMaxLength(200).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique().HasDatabaseName("idx_users_email");
        builder.Property(u => u.PasswordHash).HasColumnName("password_hash").HasMaxLength(500).IsRequired();
        builder.Property(u => u.Phone).HasColumnName("phone").HasMaxLength(20);
        builder.Property(u => u.ProfileImageUrl).HasColumnName("profile_image_url").HasMaxLength(500);
        builder.Property(u => u.CloudinaryProfileId).HasColumnName("cloudinary_profile_id").HasMaxLength(200);
        builder.Property(u => u.Status).HasColumnName("status").HasMaxLength(20).HasConversion<string>().HasDefaultValue(UserStatus.Active).IsRequired();
        builder.Property(u => u.IsEmailVerified).HasColumnName("is_email_verified").HasDefaultValue(false).IsRequired();
        builder.Property(u => u.LastLoginAt).HasColumnName("last_login_at");
        builder.Property(u => u.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(u => u.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(u => u.DeletedAt).HasColumnName("deleted_at");

        builder.HasQueryFilter(u => u.DeletedAt == null);

        builder.HasMany(u => u.UserRoles).WithOne(ur => ur.User).HasForeignKey(ur => ur.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(u => u.RefreshTokens).WithOne(rt => rt.User).HasForeignKey(rt => rt.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(u => u.PasswordResetTokens).WithOne(prt => prt.User).HasForeignKey(prt => prt.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(u => u.EmailVerificationTokens).WithOne(evt => evt.User).HasForeignKey(evt => evt.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
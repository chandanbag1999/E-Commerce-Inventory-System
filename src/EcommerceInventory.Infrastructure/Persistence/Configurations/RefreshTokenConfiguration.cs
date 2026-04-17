using EcommerceInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceInventory.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(rt => rt.Id);
        builder.Property(rt => rt.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(rt => rt.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(rt => rt.Token).HasColumnName("token").HasMaxLength(500).IsRequired();
        builder.HasIndex(rt => rt.Token).IsUnique().HasDatabaseName("idx_refresh_tokens_token");
        builder.HasIndex(rt => rt.UserId).HasDatabaseName("idx_refresh_tokens_user_id");
        builder.Property(rt => rt.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(rt => rt.IsRevoked).HasColumnName("is_revoked").HasDefaultValue(false).IsRequired();
        builder.Property(rt => rt.RevokedAt).HasColumnName("revoked_at");
        builder.Property(rt => rt.ReplacedBy).HasColumnName("replaced_by").HasMaxLength(500);
        builder.Property(rt => rt.DeviceInfo).HasColumnName("device_info").HasMaxLength(300);
        builder.Property(rt => rt.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
        builder.Ignore(rt => rt.UpdatedAt);
    }
}
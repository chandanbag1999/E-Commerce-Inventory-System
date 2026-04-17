using EcommerceInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceInventory.Infrastructure.Persistence.Configurations;

public class EmailVerificationTokenConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
{
    public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
    {
        builder.ToTable("email_verification_tokens");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(t => t.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(t => t.TokenHash).HasColumnName("token_hash").HasMaxLength(500).IsRequired();
        builder.Property(t => t.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(t => t.IsUsed).HasColumnName("is_used").HasDefaultValue(false).IsRequired();
        builder.Property(t => t.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
        builder.Ignore(t => t.UpdatedAt);
    }
}
using EcommerceInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceInventory.Infrastructure.Persistence.Configurations;

public class EmailVerificationTokenConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
{
    public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
    {
        builder.ToTable("email_verification_tokens");

        builder.HasKey(evt => evt.Id);

        builder.Property(evt => evt.TokenHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(evt => evt.ExpiresAt)
            .IsRequired();

        builder.Property(evt => evt.IsUsed)
            .IsRequired();

        builder.HasOne(evt => evt.User)
            .WithMany()
            .HasForeignKey(evt => evt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

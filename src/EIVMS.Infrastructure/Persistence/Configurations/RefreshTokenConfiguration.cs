using EIVMS.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EIVMS.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Token)
            .HasMaxLength(512)
            .IsRequired();

        builder.HasIndex(rt => rt.Token)
            .HasDatabaseName("IX_RefreshTokens_Token");

        builder.Property(rt => rt.TokenFamily)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(rt => rt.TokenFamily)
            .HasDatabaseName("IX_RefreshTokens_TokenFamily");

        builder.Property(rt => rt.RevokedReason)
            .HasMaxLength(100);

        builder.Property(rt => rt.ReplacedByToken)
            .HasMaxLength(512);

        builder.Property(rt => rt.IpAddress)
            .HasMaxLength(45)
            .IsRequired();

        builder.Property(rt => rt.UserAgent)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
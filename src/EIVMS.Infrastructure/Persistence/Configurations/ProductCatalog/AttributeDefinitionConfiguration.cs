using EIVMS.Domain.Entities.ProductCatalog;
using EIVMS.Domain.Enums.ProductCatalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EIVMS.Infrastructure.Persistence.Configurations.ProductCatalog;

public class AttributeDefinitionConfiguration : IEntityTypeConfiguration<AttributeDefinition>
{
    public void Configure(EntityTypeBuilder<AttributeDefinition> builder)
    {
        builder.ToTable("AttributeDefinitions");

        builder.HasKey(ad => ad.Id);

        builder.Property(ad => ad.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ad => ad.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(ad => ad.Code)
            .IsUnique()
            .HasDatabaseName("IX_AttributeDefinitions_Code");

        builder.Property(ad => ad.Description)
            .HasMaxLength(500);

        builder.Property(ad => ad.DataType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(ad => ad.AllowedValues)
            .HasMaxLength(1000);

        builder.Property(ad => ad.ValidationRulesJson)
            .HasMaxLength(2000);

        builder.Property(ad => ad.Unit)
            .HasMaxLength(50);

        builder.Property(ad => ad.Placeholder)
            .HasMaxLength(100);

        builder.HasOne(ad => ad.Category)
            .WithMany(c => c.AttributeDefinitions)
            .HasForeignKey(ad => ad.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(ad => ad.CategoryId);
        builder.HasIndex(ad => ad.IsActive);
        builder.HasIndex(ad => ad.IsFilterable);
    }
}
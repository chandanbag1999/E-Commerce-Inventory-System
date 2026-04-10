using EIVMS.Domain.Entities.ProductCatalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EIVMS.Infrastructure.Persistence.Configurations.ProductCatalog;

public class ProductAttributeValueConfiguration : IEntityTypeConfiguration<ProductAttributeValue>
{
    public void Configure(EntityTypeBuilder<ProductAttributeValue> builder)
    {
        builder.ToTable("ProductAttributeValues");

        builder.HasKey(pav => pav.Id);

        builder.Property(pav => pav.Value)
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasOne(pav => pav.Product)
            .WithMany(p => p.AttributeValues)
            .HasForeignKey(pav => pav.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pav => pav.AttributeDefinition)
            .WithMany(ad => ad.ProductAttributeValues)
            .HasForeignKey(pav => pav.AttributeDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pav => pav.ProductId);
        builder.HasIndex(pav => pav.AttributeDefinitionId);
        builder.HasIndex(pav => new { pav.ProductId, pav.AttributeDefinitionId })
            .IsUnique()
            .HasDatabaseName("IX_ProductAttributeValues_Product_Attribute");
    }
}
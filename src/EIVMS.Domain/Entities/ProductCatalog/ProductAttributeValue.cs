using EIVMS.Domain.Common;

namespace EIVMS.Domain.Entities.ProductCatalog;

public class ProductAttributeValue : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Guid AttributeDefinitionId { get; private set; }
    public string Value { get; private set; } = string.Empty;

    public Product Product { get; private set; } = null!;
    public AttributeDefinition AttributeDefinition { get; private set; } = null!;

    private ProductAttributeValue() { }

    public static ProductAttributeValue Create(Guid productId, Guid attributeDefinitionId, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Attribute value cannot be empty");

        return new ProductAttributeValue
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            AttributeDefinitionId = attributeDefinitionId,
            Value = value.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be empty");
        Value = value.Trim();
        SetUpdatedAt();
    }
}

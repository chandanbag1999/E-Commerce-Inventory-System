using EIVMS.Domain.Common;
using EIVMS.Domain.Enums.ProductCatalog;

namespace EIVMS.Domain.Entities.ProductCatalog;

public class AttributeDefinition : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public AttributeDataType DataType { get; private set; }
    public bool IsRequired { get; private set; } = false;
    public bool IsFilterable { get; private set; } = true;
    public bool IsVariantAttribute { get; private set; } = false;
    public string? AllowedValues { get; private set; }
    public Guid? CategoryId { get; private set; }
    public string? ValidationRulesJson { get; private set; }
    public string? Unit { get; private set; }
    public string? Placeholder { get; private set; }
    public int DisplayOrder { get; private set; } = 0;
    public bool IsActive { get; private set; } = true;

    public Category? Category { get; private set; }
    public ICollection<ProductAttributeValue> ProductAttributeValues { get; private set; } = new List<ProductAttributeValue>();

    private AttributeDefinition() { }

    public static AttributeDefinition Create(
        string name, string code, AttributeDataType dataType,
        Guid? categoryId = null, bool isRequired = false,
        bool isFilterable = true, bool isVariantAttribute = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Attribute name is required");
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Attribute code is required");

        var cleanCode = System.Text.RegularExpressions.Regex.Replace(code.ToLowerInvariant().Trim(), @"[^a-z0-9_]", "_");

        return new AttributeDefinition
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Code = cleanCode,
            DataType = dataType,
            CategoryId = categoryId,
            IsRequired = isRequired,
            IsFilterable = isFilterable,
            IsVariantAttribute = isVariantAttribute,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name, string? description, bool isRequired, bool isFilterable,
        bool isVariantAttribute, string? allowedValues, string? unit, int displayOrder)
    {
        Name = name.Trim();
        Description = description;
        IsRequired = isRequired;
        IsFilterable = isFilterable;
        IsVariantAttribute = isVariantAttribute;
        AllowedValues = allowedValues;
        Unit = unit;
        DisplayOrder = displayOrder;
        SetUpdatedAt();
    }

    public void SetValidationRules(string? validationRulesJson)
    {
        ValidationRulesJson = validationRulesJson;
        SetUpdatedAt();
    }

    public void Activate() { IsActive = true; SetUpdatedAt(); }
    public void Deactivate() { IsActive = false; SetUpdatedAt(); }

    public List<string> GetAllowedValuesList()
    {
        if (string.IsNullOrWhiteSpace(AllowedValues)) return new List<string>();
        return AllowedValues.Split(',').Select(v => v.Trim()).Where(v => !string.IsNullOrEmpty(v)).ToList();
    }
}

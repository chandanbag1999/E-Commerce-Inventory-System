using EIVMS.Domain.Common;

namespace EIVMS.Domain.Entities.Identity;

public class Permission : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Resource { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    private Permission() { }

    public static Permission Create(string name, string resource, string action, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Permission name cannot be empty", nameof(name));

        return new Permission
        {
            Id = Guid.NewGuid(),
            Name = name.ToLowerInvariant().Trim(),
            Resource = resource.ToLowerInvariant().Trim(),
            Action = action.ToLowerInvariant().Trim(),
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
    }
}
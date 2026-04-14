using EcommerceInventory.Domain.Common;

namespace EcommerceInventory.Domain.Entities;

/// <summary>
/// Role entity - represents a user role
/// </summary>
public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int HierarchyLevel { get; set; } = 100;
    public bool IsSystemRole { get; set; } = false;

    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

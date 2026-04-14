using EcommerceInventory.Domain.Common;

namespace EcommerceInventory.Domain.Entities;


// Permission entity representing an action that can be performed in the system
public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

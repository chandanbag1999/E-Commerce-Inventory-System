using EcommerceInventory.Domain.Common;

namespace EcommerceInventory.Domain.Entities;

public class Role : BaseEntity
{
    public string Name           { get; set; } = string.Empty;
    public string? Description   { get; set; }
    public int HierarchyLevel    { get; set; } = 100;
    public bool IsSystemRole     { get; set; } = false;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<UserRole>       UserRoles        { get; set; } = new List<UserRole>();

    public Role() { }
}
namespace EcommerceInventory.Domain.Entities;

// Join entity representing the many-to-many relationship between Role and Permission
public class RolePermission
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }

    // Navigation properties
    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}

using MediatR;

namespace EcommerceInventory.Application.Features.Roles.Queries.GetAllRoles;

public class GetAllRolesQuery : IRequest<List<RoleDto>>
{
}

public class RoleDto
{
    public Guid   Id             { get; set; }
    public string Name           { get; set; } = string.Empty;
    public string? Description   { get; set; }
    public int    HierarchyLevel { get; set; }
    public bool   IsSystemRole   { get; set; }
}

using MediatR;

namespace EcommerceInventory.Application.Features.Roles.Queries.GetAllPermissions;

public class GetAllPermissionsQuery : IRequest<List<PermissionDto>>
{
}

public class PermissionDto
{
    public Guid   Id          { get; set; }
    public string Name        { get; set; } = string.Empty;
    public string Module      { get; set; } = string.Empty;
    public string? Description { get; set; }
}

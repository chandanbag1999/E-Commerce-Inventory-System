using EcommerceInventory.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Roles.Queries.GetAllRoles;

public class GetAllRolesQueryHandler
    : IRequestHandler<GetAllRolesQuery, List<RoleDto>>
{
    private readonly IUnitOfWork _uow;

    public GetAllRolesQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<RoleDto>> Handle(
        GetAllRolesQuery request,
        CancellationToken cancellationToken)
    {
        var roles = await _uow.Roles.Query()
            .OrderBy(r => r.HierarchyLevel)
            .ToListAsync(cancellationToken);

        return roles.Select(r => new RoleDto
        {
            Id             = r.Id,
            Name           = r.Name,
            Description    = r.Description,
            HierarchyLevel = r.HierarchyLevel,
            IsSystemRole   = r.IsSystemRole
        }).ToList();
    }
}

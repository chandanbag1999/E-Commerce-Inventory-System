using EcommerceInventory.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Roles.Queries.GetAllPermissions;

public class GetAllPermissionsQueryHandler
    : IRequestHandler<GetAllPermissionsQuery, List<PermissionDto>>
{
    private readonly IUnitOfWork _uow;

    public GetAllPermissionsQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<PermissionDto>> Handle(
        GetAllPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var permissions = await _uow.Permissions.Query()
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return permissions.Select(p => new PermissionDto
        {
            Id          = p.Id,
            Name        = p.Name,
            Module      = p.Module,
            Description = p.Description
        }).ToList();
    }
}

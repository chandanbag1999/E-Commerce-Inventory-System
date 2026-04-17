using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Roles.Queries.GetAllPermissions;
using EcommerceInventory.Application.Features.Roles.Queries.GetAllRoles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceInventory.API.Controllers;

[Authorize]
public class RolesController : BaseApiController
{
    // GET /api/v1/roles
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetAllRolesQuery(), ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    // GET /api/v1/permissions
    [HttpGet("/api/v1/permissions")]
    public async Task<IActionResult> GetPermissions(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetAllPermissionsQuery(), ct);
        return Ok(ApiResponse<object>.Ok(result));
    }
}

using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Warehouses.Commands.ActivateWarehouse;
using EcommerceInventory.Application.Features.Warehouses.Commands.CreateWarehouse;
using EcommerceInventory.Application.Features.Warehouses.Commands.DeactivateWarehouse;
using EcommerceInventory.Application.Features.Warehouses.Commands.DeleteWarehouse;
using EcommerceInventory.Application.Features.Warehouses.Commands.UpdateWarehouse;
using EcommerceInventory.Application.Features.Warehouses.Queries.GetAllWarehouses;
using EcommerceInventory.Application.Features.Warehouses.Queries.GetWarehouseById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceInventory.API.Controllers;

[Authorize]
public class WarehousesController : BaseApiController
{
    // GET /api/v1/warehouses
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin,InventoryManager,WarehouseManager,PurchaseManager,SalesManager")]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? isActive,
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetAllWarehousesQuery
            {
                IsActive   = isActive,
                Search     = search,
                PageNumber = pageNumber,
                PageSize   = pageSize
            }, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    // GET /api/v1/warehouses/{id}
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,InventoryManager,WarehouseManager,PurchaseManager,SalesManager")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new GetWarehouseByIdQuery { Id = id }, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    // POST /api/v1/warehouses
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,InventoryManager,WarehouseManager")]
    public async Task<IActionResult> Create(
        [FromBody] CreateWarehouseCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return StatusCode(201, ApiResponse<object>.Ok(
            result, "Warehouse created successfully."));
    }

    // PUT /api/v1/warehouses/{id}
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,InventoryManager,WarehouseManager")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateWarehouseCommand command,
        CancellationToken ct)
    {
        command.Id = id;
        var result = await Mediator.Send(command, ct);
        return Ok(ApiResponse<object>.Ok(
            result, "Warehouse updated successfully."));
    }

    // PATCH /api/v1/warehouses/{id}/activate
    [HttpPatch("{id:guid}/activate")]
    [Authorize(Roles = "SuperAdmin,Admin,InventoryManager,WarehouseManager")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new ActivateWarehouseCommand { Id = id }, ct);
        return Ok(ApiResponse<object>.Ok(
            result, "Warehouse activated successfully."));
    }

    // PATCH /api/v1/warehouses/{id}/deactivate
    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Roles = "SuperAdmin,Admin,InventoryManager,WarehouseManager")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new DeactivateWarehouseCommand { Id = id }, ct);
        return Ok(ApiResponse<object>.Ok(
            result, "Warehouse deactivated successfully."));
    }

    // DELETE /api/v1/warehouses/{id}
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteWarehouseCommand { Id = id }, ct);
        return Ok(ApiResponse.Ok("Warehouse deleted successfully."));
    }
}

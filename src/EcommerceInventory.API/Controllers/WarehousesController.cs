using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Warehouses.Commands.CreateWarehouse;
using EcommerceInventory.Application.Features.Warehouses.Commands.DeleteWarehouse;
using EcommerceInventory.Application.Features.Warehouses.Commands.UpdateWarehouse;
using EcommerceInventory.Application.Features.Warehouses.DTOs;
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
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? isActive,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new GetAllWarehousesQuery { IsActive = isActive }, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    // GET /api/v1/warehouses/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new GetWarehouseByIdQuery { Id = id }, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    // POST /api/v1/warehouses
    [HttpPost]
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

    // DELETE /api/v1/warehouses/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteWarehouseCommand { Id = id }, ct);
        return Ok(ApiResponse.Ok("Warehouse deleted successfully."));
    }
}

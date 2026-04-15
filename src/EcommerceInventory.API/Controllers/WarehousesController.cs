using EcommerceInventory.API.Authorization;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Warehouses.Commands.CreateWarehouse;
using EcommerceInventory.Application.Features.Warehouses.Commands.DeleteWarehouse;
using EcommerceInventory.Application.Features.Warehouses.Commands.UpdateWarehouse;
using EcommerceInventory.Application.Features.Warehouses.DTOs;
using EcommerceInventory.Application.Features.Warehouses.Queries.GetAllWarehouses;
using EcommerceInventory.Application.Features.Warehouses.Queries.GetWarehouseById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceInventory.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class WarehousesController : ControllerBase
{
    private readonly IMediator _mediator;

    public WarehousesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission("Warehouses.View")]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllWarehousesQuery(), ct);
        return Ok(new ApiResponse<List<WarehouseDto>>(true, result.Data!));
    }

    [HasPermission("Warehouses.View")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetWarehouseByIdQuery(id), ct);
        return result.Success ? Ok(new ApiResponse<WarehouseDto>(true, result.Data!)) : NotFound(new ApiResponse<object>(false, result.Message!));
    }

    [HasPermission("Warehouses.Create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseDto dto, CancellationToken ct)
    {
        var command = new CreateWarehouseCommand
        {
            Name = dto.Name,
            Code = dto.Code,
            Street = dto.Street,
            City = dto.City,
            State = dto.State,
            Pincode = dto.Pincode,
            Country = dto.Country,
            ManagerId = dto.ManagerId,
            Phone = dto.Phone
        };
        var result = await _mediator.Send(command, ct);
        return result.Success ? Created(string.Empty, new ApiResponse<WarehouseDto>(true, result.Data!, result.Message)) : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message));
    }

    [HasPermission("Warehouses.Edit")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWarehouseDto dto, CancellationToken ct)
    {
        var command = new UpdateWarehouseCommand
        {
            Id = id,
            Name = dto.Name,
            Code = dto.Code,
            Street = dto.Street,
            City = dto.City,
            State = dto.State,
            Pincode = dto.Pincode,
            Country = dto.Country,
            ManagerId = dto.ManagerId,
            Phone = dto.Phone
        };
        var result = await _mediator.Send(command, ct);
        return result.Success ? Ok(new ApiResponse<WarehouseDto>(true, result.Data!, result.Message)) : NotFound(new ApiResponse<object>(false, result.Message!));
    }

    [HasPermission("Warehouses.Delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteWarehouseCommand(id), ct);
        return result.Success ? Ok(new ApiResponse<bool>(true, result.Data!, result.Message)) : NotFound(new ApiResponse<object>(false, result.Message!));
    }
}

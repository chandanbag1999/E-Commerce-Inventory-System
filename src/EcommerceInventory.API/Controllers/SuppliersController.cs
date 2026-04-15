using EcommerceInventory.API.Authorization;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Suppliers.Commands.CreateSupplier;
using EcommerceInventory.Application.Features.Suppliers.Commands.DeleteSupplier;
using EcommerceInventory.Application.Features.Suppliers.Commands.UpdateSupplier;
using EcommerceInventory.Application.Features.Suppliers.DTOs;
using EcommerceInventory.Application.Features.Suppliers.Queries.GetAllSuppliers;
using EcommerceInventory.Application.Features.Suppliers.Queries.GetSupplierById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceInventory.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly IMediator _mediator;

    public SuppliersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HasPermission("Suppliers.View")]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllSuppliersQuery(), ct);
        return Ok(new ApiResponse<List<SupplierDto>>(true, result.Data!));
    }

    [HasPermission("Suppliers.View")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSupplierByIdQuery(id), ct);
        return result.Success ? Ok(new ApiResponse<SupplierDto>(true, result.Data!)) : NotFound(new ApiResponse<object>(false, result.Message!));
    }

    [HasPermission("Suppliers.Create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupplierDto dto, CancellationToken ct)
    {
        var command = new CreateSupplierCommand
        {
            Name = dto.Name,
            ContactName = dto.ContactName,
            Email = dto.Email,
            Phone = dto.Phone,
            Street = dto.Street,
            City = dto.City,
            State = dto.State,
            Pincode = dto.Pincode,
            Country = dto.Country,
            GstNumber = dto.GstNumber
        };
        var result = await _mediator.Send(command, ct);
        return result.Success ? Created(string.Empty, new ApiResponse<SupplierDto>(true, result.Data!, result.Message)) : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message));
    }

    [HasPermission("Suppliers.Edit")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSupplierDto dto, CancellationToken ct)
    {
        var command = new UpdateSupplierCommand
        {
            Id = id,
            Name = dto.Name,
            ContactName = dto.ContactName,
            Email = dto.Email,
            Phone = dto.Phone,
            Street = dto.Street,
            City = dto.City,
            State = dto.State,
            Pincode = dto.Pincode,
            Country = dto.Country,
            GstNumber = dto.GstNumber
        };
        var result = await _mediator.Send(command, ct);
        return result.Success ? Ok(new ApiResponse<SupplierDto>(true, result.Data!, result.Message)) : NotFound(new ApiResponse<object>(false, result.Message!));
    }

    [HasPermission("Suppliers.Delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteSupplierCommand(id), ct);
        return result.Success ? Ok(new ApiResponse<bool>(true, result.Data!, result.Message)) : NotFound(new ApiResponse<object>(false, result.Message!));
    }
}

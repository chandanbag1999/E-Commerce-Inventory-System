using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Suppliers.Commands.CreateSupplier;
using EcommerceInventory.Application.Features.Suppliers.Commands.DeleteSupplier;
using EcommerceInventory.Application.Features.Suppliers.Commands.UpdateSupplier;
using EcommerceInventory.Application.Features.Suppliers.Queries.GetAllSuppliers;
using EcommerceInventory.Application.Features.Suppliers.Queries.GetSupplierById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceInventory.API.Controllers;

[Authorize]
public class SuppliersController : BaseApiController
{
    // GET /api/v1/suppliers
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetAllSuppliersQuery query,
        CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    // GET /api/v1/suppliers/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new GetSupplierByIdQuery { Id = id }, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    // POST /api/v1/suppliers
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSupplierCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return StatusCode(201, ApiResponse<object>.Ok(
            result, "Supplier created successfully."));
    }

    // PUT /api/v1/suppliers/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateSupplierCommand command,
        CancellationToken ct)
    {
        command.Id = id;
        var result = await Mediator.Send(command, ct);
        return Ok(ApiResponse<object>.Ok(
            result, "Supplier updated successfully."));
    }

    // DELETE /api/v1/suppliers/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteSupplierCommand { Id = id }, ct);
        return Ok(ApiResponse.Ok("Supplier deleted successfully."));
    }
}

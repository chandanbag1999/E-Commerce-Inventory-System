using System.Security.Claims;
using EIVMS.Application.Modules.UserManagement.DTOs.Address;
using EIVMS.Application.Modules.UserManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EIVMS.API.Controllers.v1.UserManagement;

[ApiController]
[Route("api/v1/addresses")]
[Authorize]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    private Guid GetCurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetMyAddresses()
    {
        var result = await _addressService.GetUserAddressesAsync(GetCurrentUserId());
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAddress(Guid id)
    {
        var result = await _addressService.GetAddressByIdAsync(id, GetCurrentUserId());
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAddress([FromBody] CreateAddressDto dto)
    {
        var result = await _addressService.CreateAddressAsync(GetCurrentUserId(), dto);
        return result.Success ? StatusCode(201, result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAddress(Guid id, [FromBody] CreateAddressDto dto)
    {
        var result = await _addressService.UpdateAddressAsync(id, GetCurrentUserId(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAddress(Guid id)
    {
        var result = await _addressService.DeleteAddressAsync(id, GetCurrentUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}/set-default")]
    public async Task<IActionResult> SetDefault(Guid id)
    {
        var result = await _addressService.SetDefaultAddressAsync(id, GetCurrentUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
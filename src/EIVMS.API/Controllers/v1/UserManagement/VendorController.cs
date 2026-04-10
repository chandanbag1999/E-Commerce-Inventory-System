using System.Security.Claims;
using EIVMS.Application.Modules.UserManagement.DTOs.Vendor;
using EIVMS.Application.Modules.UserManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EIVMS.API.Controllers.v1.UserManagement;

[ApiController]
[Route("api/v1/vendors")]
[Authorize]
public class VendorController : ControllerBase
{
    private readonly IVendorService _vendorService;

    public VendorController(IVendorService vendorService)
    {
        _vendorService = vendorService;
    }

    private Guid GetCurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("apply")]
    public async Task<IActionResult> Apply([FromBody] CreateVendorApplicationDto dto)
    {
        var result = await _vendorService.ApplyForVendorAsync(dto, GetCurrentUserId());
        return result.Success ? StatusCode(201, result) : BadRequest(result);
    }

    [HttpGet("my-application")]
    public async Task<IActionResult> GetMyApplication()
    {
        var result = await _vendorService.GetMyApplicationAsync(GetCurrentUserId());
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("applications/{id:guid}/bank-details")]
    public async Task<IActionResult> UpdateBankDetails(Guid id, [FromBody] UpdateBankDetailsDto dto)
    {
        var result = await _vendorService.UpdateBankDetailsAsync(id, dto, GetCurrentUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("applications/{id:guid}/submit")]
    public async Task<IActionResult> Submit(Guid id)
    {
        var result = await _vendorService.SubmitApplicationAsync(id, GetCurrentUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("applications")]
    [Authorize(Policy = "user:manage")]
    public async Task<IActionResult> GetAllApplications([FromQuery] string? status = null)
    {
        var result = await _vendorService.GetAllApplicationsAsync(status);
        return Ok(result);
    }

    [HttpGet("applications/{id:guid}")]
    [Authorize(Policy = "user:manage")]
    public async Task<IActionResult> GetApplication(Guid id)
    {
        var result = await _vendorService.GetApplicationByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("applications/{id:guid}/review")]
    [Authorize(Policy = "user:manage")]
    public async Task<IActionResult> Review(Guid id, [FromBody] ReviewVendorApplicationDto dto)
    {
        var result = await _vendorService.ReviewApplicationAsync(id, dto, GetCurrentUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
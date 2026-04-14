using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceInventory.API.Controllers;

/// <summary>
/// Base API controller with common helper methods
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected ICurrentUserService CurrentUser => HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();

    /// <summary>
    /// Returns a success response with data
    /// </summary>
    protected IActionResult Success<T>(T data, string? message = null)
    {
        return Ok(new ApiResponse<T>(true, data, message));
    }

    /// <summary>
    /// Returns a created resource response
    /// </summary>
    protected IActionResult Created<T>(string routeName, object routeValues, T data, string? message = null)
    {
        var response = new ApiResponse<T>(true, data, message);
        return CreatedAtRoute(routeName, routeValues, response);
    }

    /// <summary>
    /// Returns a bad request response
    /// </summary>
    protected IActionResult BadRequest(string message)
    {
        return base.BadRequest(new ApiResponse<object>(false, message));
    }

    /// <summary>
    /// Returns a not found response
    /// </summary>
    protected IActionResult NotFound(string message)
    {
        return base.NotFound(new ApiResponse<object>(false, message));
    }

    /// <summary>
    /// Returns the current user ID from JWT
    /// </summary>
    protected Guid GetCurrentUserId()
    {
        var userId = CurrentUser.UserId;
        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return userId.Value;
    }
}

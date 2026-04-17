using EcommerceInventory.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceInventory.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _mediator;

    protected ISender Mediator
        => _mediator ??= HttpContext.RequestServices
                                    .GetRequiredService<ISender>();

    protected ActionResult<ApiResponse<T>> OkResponse<T>(
        T data, string? message = null)
        => Ok(ApiResponse<T>.Ok(data, message));

    protected ActionResult<ApiResponse<T>> CreatedResponse<T>(
        T data, string? message = null)
        => StatusCode(201, ApiResponse<T>.Ok(data, message));

    protected ActionResult<ApiResponse<T>> NotFoundResponse<T>(
        string message)
        => NotFound(ApiResponse<T>.Fail(message));

    protected ActionResult BadRequestResponse(string message)
        => BadRequest(ApiResponse.Fail(message));
}
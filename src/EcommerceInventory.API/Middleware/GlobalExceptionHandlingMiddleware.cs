using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Domain.Exceptions;
using FluentValidation;
using System.Text.Json;

namespace EcommerceInventory.API.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        _logger.LogError(ex,
            "Unhandled exception: {Message}", ex.Message);

        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = ex switch
        {
            ValidationException ve => (
                StatusCodes.Status400BadRequest,
                "Validation failed.",
                (object)ve.Errors
                          .GroupBy(e => e.PropertyName)
                          .ToDictionary(
                              g => g.Key,
                              g => g.Select(e => e.ErrorMessage).ToArray())
            ),

            NotFoundException nfe => (
                StatusCodes.Status404NotFound,
                nfe.Message,
                (object?)null
            ),

            UnauthorizedException ue => (
                StatusCodes.Status401Unauthorized,
                ue.Message,
                (object?)null
            ),

            BusinessRuleViolationException bre => (
                StatusCodes.Status422UnprocessableEntity,
                bre.Message,
                (object?)null
            ),

            DomainException de => (
                StatusCodes.Status400BadRequest,
                de.Message,
                (object?)null
            ),

            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized.",
                (object?)null
            ),

            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred. Please try again later.",
                (object?)null
            )
        };

        context.Response.StatusCode = statusCode;

        var response = new
        {
            success = false,
            message,
            errors
        };

        var json = JsonSerializer.Serialize(response,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        await context.Response.WriteAsync(json);
    }
}
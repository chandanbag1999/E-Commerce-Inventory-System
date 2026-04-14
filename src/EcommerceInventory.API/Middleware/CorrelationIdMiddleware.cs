using Serilog;

namespace EcommerceInventory.API.Middleware;

/// <summary>
/// Correlation ID middleware - adds a unique ID to each request for tracing
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault();

        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        context.Response.Headers[CorrelationIdHeader] = correlationId;
        context.Items["CorrelationId"] = correlationId;

        // Log request details
        Log.Information(
            "[{CorrelationId}] {Method} {Path} {QueryString}",
            correlationId,
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString);

        var startTime = DateTime.UtcNow;
        await _next(context);
        var elapsed = DateTime.UtcNow - startTime;

        // Log response details
        Log.Information(
            "[{CorrelationId}] {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
            correlationId,
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            elapsed.TotalMilliseconds);
    }
}

/// <summary>
/// Extension method to add correlation ID middleware
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }
}

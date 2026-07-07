using Microsoft.AspNetCore.Diagnostics;

namespace TechFeed.API.Middleware;

/// <summary>
/// Catches unhandled exceptions so raw stack traces never reach the client.
/// The full exception is logged server-side; the client gets a generic 500.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Unhandled exception while processing {Method} {Path}",
            httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(
            new { error = "An unexpected error occurred" }, cancellationToken);

        return true;
    }
}

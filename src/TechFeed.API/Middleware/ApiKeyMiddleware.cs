using Microsoft.Extensions.Options;
using TechFeed.API.Configuration;

namespace TechFeed.API.Middleware;

/// <summary>
/// Guards POST /api/feed/refresh with a static API key supplied via the
/// "X-Api-Key" header. All other endpoints stay public.
/// </summary>
public class ApiKeyMiddleware
{
    private const string ApiKeyHeader = "X-Api-Key";
    private const string ProtectedPath = "/api/feed/refresh";

    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyMiddleware> _logger;
    private readonly string _configuredKey;

    public ApiKeyMiddleware(RequestDelegate next, IOptions<ApiSettings> options, ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _configuredKey = options.Value.RefreshApiKey;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (RequiresApiKey(context.Request) && !IsAuthorized(context.Request))
        {
            _logger.LogWarning(
                "Rejected {Method} {Path}: missing or invalid API key",
                context.Request.Method, context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid or missing API key" });
            return;
        }

        await _next(context);
    }

    private static bool RequiresApiKey(HttpRequest request) =>
        HttpMethods.IsPost(request.Method)
        && request.Path.Equals(ProtectedPath, StringComparison.OrdinalIgnoreCase);

    private bool IsAuthorized(HttpRequest request) =>
        !string.IsNullOrEmpty(_configuredKey)
        && request.Headers.TryGetValue(ApiKeyHeader, out var provided)
        && string.Equals(provided, _configuredKey, StringComparison.Ordinal);
}

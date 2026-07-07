using TechFeed.Core;

namespace TechFeed.API.Endpoints;

public static class FeedEndpoints
{
    private const string LoggerCategory = "TechFeed.API.Endpoints.FeedEndpoints";

    public static IEndpointRouteBuilder MapFeedEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/feed");

        group.MapPost("/refresh", async (
            IFeedRefreshService feedRefreshService,
            ICacheService cache,
            ILoggerFactory loggerFactory,
            string? tag,
            int? limit) =>
        {
            var result = await feedRefreshService.RefreshAsync(tag, limit ?? 20);

            // Invalidate cached article reads regardless of partial source errors,
            // since the underlying data may have changed.
            var logger = loggerFactory.CreateLogger(LoggerCategory);
            logger.LogInformation("Invalidating article caches after feed refresh");

            await cache.DeleteByPatternAsync("articles:*");
            await cache.DeleteByPatternAsync("article:*");

            return Results.Ok(result);
        })
        .WithName("RefreshFeed")
        .WithSummary("Fetches the latest articles from every configured source and persists them.");

        return app;
    }
}

using TechFeed.Core;

namespace TechFeed.API.Endpoints;

public static class FeedEndpoints
{
    public static IEndpointRouteBuilder MapFeedEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/feed");

        group.MapPost("/refresh", async (
            IFeedRefreshService feedRefreshService,
            string? tag,
            int? limit) =>
        {
            var result = await feedRefreshService.RefreshAsync(tag, limit ?? 20);
            return Results.Ok(result);
        })
        .WithName("RefreshFeed")
        .WithSummary("Fetches the latest articles from every configured source and persists them.");

        return app;
    }
}

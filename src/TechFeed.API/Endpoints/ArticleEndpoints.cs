using TechFeed.API.Mapping;
using TechFeed.Core;
using TechFeed.Shared;

namespace TechFeed.API.Endpoints;

public static class ArticleEndpoints
{
    private const int DefaultLimit = 20;
    private const int MaxLimit = 100;

    private static readonly string[] KnownSources = ["devto", "hackernews"];

    public static IEndpointRouteBuilder MapArticleEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/articles", async (
            IArticleRepository repository,
            string? tag,
            string? source,
            int? limit) =>
        {
            var requestedLimit = limit ?? DefaultLimit;

            if (requestedLimit < 0)
            {
                return Results.BadRequest(new { message = "limit must be zero or greater." });
            }

            // Clamp oversized requests instead of rejecting them.
            var effectiveLimit = Math.Min(requestedLimit, MaxLimit);

            var articles = await repository.GetAllAsync(tag, source, effectiveLimit);
            var totalCount = await repository.CountAsync(tag, source);

            var items = articles.Select(a => a.ToResponse()).ToList();

            return Results.Ok(new PagedResponse<ArticleResponse>(items, totalCount, effectiveLimit));
        })
        .WithName("GetArticles")
        .WithSummary("Lists stored articles, optionally filtered by tag and source.")
        .Produces<PagedResponse<ArticleResponse>>()
        .Produces(StatusCodes.Status400BadRequest);

        app.MapGet("/api/articles/{id}", async (
            IArticleRepository repository,
            string id) =>
        {
            var article = await repository.GetByIdAsync(id);

            return article is null
                ? Results.NotFound(new { message = $"Article '{id}' was not found." })
                : Results.Ok(article.ToResponse());
        })
        .WithName("GetArticleById")
        .WithSummary("Returns a single article by its id.")
        .Produces<ArticleResponse>()
        .Produces(StatusCodes.Status404NotFound);

        app.MapGet("/api/sources", () => Results.Ok(KnownSources))
            .WithName("GetSources")
            .WithSummary("Lists the supported article sources.")
            .Produces<string[]>();

        return app;
    }
}

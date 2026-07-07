using TechFeed.API.Mapping;
using TechFeed.Core;
using TechFeed.Shared;

namespace TechFeed.API.Endpoints;

public static class ArticleEndpoints
{
    private const int DefaultLimit = 20;
    private const int MaxLimit = 100;
    private const string LoggerCategory = "TechFeed.API.Endpoints.ArticleEndpoints";

    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);
    private static readonly string[] KnownSources = ["devto", "hackernews"];

    public static IEndpointRouteBuilder MapArticleEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/articles", async (
            IArticleRepository repository,
            ICacheService cache,
            ILoggerFactory loggerFactory,
            string? tag,
            string? source,
            int? limit) =>
        {
            var logger = loggerFactory.CreateLogger(LoggerCategory);

            var requestedLimit = limit ?? DefaultLimit;
            if (requestedLimit < 0)
            {
                return Results.BadRequest(new { message = "limit must be zero or greater." });
            }

            // Clamp oversized requests instead of rejecting them.
            var effectiveLimit = Math.Min(requestedLimit, MaxLimit);

            var cacheKey = $"articles:{tag ?? "all"}:{source ?? "all"}:{effectiveLimit}";

            var cached = await cache.GetAsync<PagedResponse<ArticleResponse>>(cacheKey);
            if (cached is not null)
            {
                logger.LogInformation("CACHE HIT for {CacheKey}", cacheKey);
                return Results.Ok(cached);
            }

            logger.LogInformation("CACHE MISS for {CacheKey} — querying MongoDB", cacheKey);

            var articles = await repository.GetAllAsync(tag, source, effectiveLimit);
            var totalCount = await repository.CountAsync(tag, source);

            var items = articles.Select(a => a.ToResponse()).ToList();
            var response = new PagedResponse<ArticleResponse>(items, totalCount, effectiveLimit);

            await cache.SetAsync(cacheKey, response, CacheTtl);

            return Results.Ok(response);
        })
        .WithName("GetArticles")
        .WithSummary("Lists stored articles, optionally filtered by tag and source.")
        .Produces<PagedResponse<ArticleResponse>>()
        .Produces(StatusCodes.Status400BadRequest);

        app.MapGet("/api/articles/{id}", async (
            IArticleRepository repository,
            ICacheService cache,
            ILoggerFactory loggerFactory,
            string id) =>
        {
            var logger = loggerFactory.CreateLogger(LoggerCategory);
            var cacheKey = $"article:{id}";

            var cached = await cache.GetAsync<ArticleResponse>(cacheKey);
            if (cached is not null)
            {
                logger.LogInformation("CACHE HIT for {CacheKey}", cacheKey);
                return Results.Ok(cached);
            }

            logger.LogInformation("CACHE MISS for {CacheKey} — querying MongoDB", cacheKey);

            var article = await repository.GetByIdAsync(id);
            if (article is null)
            {
                return Results.NotFound(new { message = $"Article '{id}' was not found." });
            }

            var response = article.ToResponse();
            await cache.SetAsync(cacheKey, response, CacheTtl);

            return Results.Ok(response);
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

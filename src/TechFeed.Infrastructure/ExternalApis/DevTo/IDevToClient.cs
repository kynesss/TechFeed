using Refit;

namespace TechFeed.Infrastructure.ExternalApis.DevTo;

/// <summary>
/// Refit client for the dev.to public API. Base address (https://dev.to)
/// is configured during DI registration.
/// </summary>
public interface IDevToClient
{
    [Get("/api/articles?tag={tag}&per_page={limit}&top=7")]
    Task<List<DevToArticleDto>> GetArticlesAsync(string tag, int limit);
}

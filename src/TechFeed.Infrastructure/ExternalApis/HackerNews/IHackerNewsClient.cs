using Refit;

namespace TechFeed.Infrastructure.ExternalApis.HackerNews;

/// <summary>
/// Refit client for the Hacker News Firebase API. Base address
/// (https://hacker-news.firebaseio.com) is configured during DI registration.
/// </summary>
public interface IHackerNewsClient
{
    [Get("/v0/topstories.json")]
    Task<List<int>> GetTopStoryIdsAsync();

    [Get("/v0/item/{id}.json")]
    Task<HackerNewsItemDto?> GetItemAsync(int id);
}

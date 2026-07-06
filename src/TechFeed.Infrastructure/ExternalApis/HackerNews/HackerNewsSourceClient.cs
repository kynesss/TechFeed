using TechFeed.Core;

namespace TechFeed.Infrastructure.ExternalApis.HackerNews;

public class HackerNewsSourceClient : IArticleSourceClient
{
    private readonly IHackerNewsClient _client;

    public HackerNewsSourceClient(IHackerNewsClient client)
    {
        _client = client;
    }

    public string SourceName => "hackernews";

    // Hacker News has no notion of tags, so the tag argument is ignored.
    public async Task<List<Article>> FetchLatestAsync(string? tag, int limit)
    {
        var topStoryIds = await _client.GetTopStoryIdsAsync();

        var items = await Task.WhenAll(
            topStoryIds.Take(limit).Select(id => _client.GetItemAsync(id)));

        return items
            .Where(item => item is { Type: "story" } && !string.IsNullOrWhiteSpace(item.Url))
            .Select(item => Map(item!))
            .ToList();
    }

    private Article Map(HackerNewsItemDto item)
    {
        var now = DateTime.UtcNow;

        return new Article
        {
            ExternalId = item.Id.ToString(),
            Source = SourceName,
            Title = item.Title ?? string.Empty,
            Url = item.Url ?? string.Empty,
            CoverImage = null,
            Description = null,
            Tags = new List<string>(),
            Score = item.Score,
            PublishedAt = item.PublishedAt,
            FetchedAt = now
        };
    }
}

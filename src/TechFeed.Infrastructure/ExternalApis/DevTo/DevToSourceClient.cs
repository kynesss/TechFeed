using TechFeed.Core;

namespace TechFeed.Infrastructure.ExternalApis.DevTo;

public class DevToSourceClient(IDevToClient client) : IArticleSourceClient
{
    private const string DefaultTag = "dotnet";
    public string SourceName => "devto";

    public async Task<List<Article>> FetchLatestAsync(string? tag, int limit)
    {
        var effectiveTag = string.IsNullOrWhiteSpace(tag) ? DefaultTag : tag;
        var dtos = await client.GetArticlesAsync(effectiveTag, limit);

        return dtos.Select(Map).ToList();
    }

    private Article Map(DevToArticleDto dto)
    {
        var now = DateTime.UtcNow;

        return new Article
        {
            ExternalId = dto.Id.ToString(),
            Source = SourceName,
            Title = dto.Title,
            Url = dto.Url,
            CoverImage = dto.CoverImage,
            Description = dto.Description,
            Tags = new List<string>(dto.TagList),
            Score = dto.PositiveReactionsCount,
            PublishedAt = dto.PublishedAt,
            FetchedAt = now
        };
    }
}

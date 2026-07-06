namespace TechFeed.Core;

/// <summary>
/// Unifies the different external article providers (dev.to, Hacker News, ...)
/// under a single contract the business layer can consume.
/// </summary>
public interface IArticleSourceClient
{
    string SourceName { get; }

    Task<List<Article>> FetchLatestAsync(string? tag, int limit);
}

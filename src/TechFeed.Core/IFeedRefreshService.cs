namespace TechFeed.Core;

/// <summary>
/// Orchestrates fetching articles from every configured source and persisting
/// them, returning a summary of what changed.
/// </summary>
public interface IFeedRefreshService
{
    Task<FeedRefreshResult> RefreshAsync(string? tag, int limitPerSource);
}

namespace TechFeed.Core;

public record FeedRefreshResult(
    int TotalFetched,
    int NewArticles,
    int UpdatedArticles,
    List<string> Errors);

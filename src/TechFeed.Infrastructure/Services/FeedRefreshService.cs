using Microsoft.Extensions.Logging;
using TechFeed.Core;

namespace TechFeed.Infrastructure.Services;

public class FeedRefreshService : IFeedRefreshService
{
    private readonly IEnumerable<IArticleSourceClient> _sourceClients;
    private readonly IArticleRepository _repository;
    private readonly ILogger<FeedRefreshService> _logger;

    public FeedRefreshService(
        IEnumerable<IArticleSourceClient> sourceClients,
        IArticleRepository repository,
        ILogger<FeedRefreshService> logger)
    {
        _sourceClients = sourceClients;
        _repository = repository;
        _logger = logger;
    }

    public async Task<FeedRefreshResult> RefreshAsync(string? tag, int limitPerSource)
    {
        _logger.LogInformation(
            "Feed refresh started (tag: {Tag}, limitPerSource: {Limit})",
            tag ?? "<none>", limitPerSource);

        var errors = new List<string>();

        // Fetch from all sources in parallel. Each source is wrapped so a
        // single failing provider does not abort the whole refresh.
        var fetchResults = await Task.WhenAll(
            _sourceClients.Select(client => FetchFromSourceAsync(client, tag, limitPerSource)));

        var articles = new List<Article>();
        foreach (var (sourceName, sourceArticles, error) in fetchResults)
        {
            if (error is not null)
            {
                errors.Add(error);
                continue;
            }

            _logger.LogInformation(
                "Source {Source} returned {Count} articles", sourceName, sourceArticles.Count);
            articles.AddRange(sourceArticles);
        }

        var newArticles = 0;
        var updatedArticles = 0;

        foreach (var article in articles)
        {
            article.FetchedAt = DateTime.UtcNow;

            // Check existence before upserting so we can distinguish new from
            // updated articles in the summary.
            var exists = await _repository.ExistsAsync(article.ExternalId, article.Source);
            if (exists)
            {
                updatedArticles++;
            }
            else
            {
                newArticles++;
            }

            await _repository.UpsertAsync(article);
        }

        var result = new FeedRefreshResult(articles.Count, newArticles, updatedArticles, errors);

        _logger.LogInformation(
            "Feed refresh finished: fetched {Total}, new {New}, updated {Updated}, errors {Errors}",
            result.TotalFetched, result.NewArticles, result.UpdatedArticles, errors.Count);

        return result;
    }

    private async Task<(string SourceName, List<Article> Articles, string? Error)> FetchFromSourceAsync(
        IArticleSourceClient client, string? tag, int limit)
    {
        try
        {
            var articles = await client.FetchLatestAsync(tag, limit);
            return (client.SourceName, articles, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Source {Source} failed during refresh", client.SourceName);
            return (client.SourceName, new List<Article>(), $"Source '{client.SourceName}' failed: {ex.Message}");
        }
    }
}

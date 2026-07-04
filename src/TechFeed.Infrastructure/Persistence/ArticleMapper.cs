using TechFeed.Core;

namespace TechFeed.Infrastructure.Persistence;

/// <summary>
/// Maps between the domain <see cref="Article"/> and the persistence
/// <see cref="ArticleDocument"/>.
/// </summary>
public static class ArticleMapper
{
    public static ArticleDocument ToDocument(this Article article)
    {
        return new ArticleDocument
        {
            // An empty Id means "not yet persisted" — leave it null so Mongo
            // assigns a fresh ObjectId on insert.
            Id = string.IsNullOrEmpty(article.Id) ? null : article.Id,
            ExternalId = article.ExternalId,
            Source = article.Source,
            Title = article.Title,
            Url = article.Url,
            CoverImage = article.CoverImage,
            Description = article.Description,
            Tags = new List<string>(article.Tags),
            Score = article.Score,
            PublishedAt = article.PublishedAt,
            FetchedAt = article.FetchedAt
        };
    }

    public static Article ToDomain(this ArticleDocument document)
    {
        return new Article
        {
            Id = document.Id ?? string.Empty,
            ExternalId = document.ExternalId,
            Source = document.Source,
            Title = document.Title,
            Url = document.Url,
            CoverImage = document.CoverImage,
            Description = document.Description,
            Tags = new List<string>(document.Tags),
            Score = document.Score,
            PublishedAt = document.PublishedAt,
            FetchedAt = document.FetchedAt
        };
    }
}

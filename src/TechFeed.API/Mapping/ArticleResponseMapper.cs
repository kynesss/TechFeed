using TechFeed.Core;
using TechFeed.Shared;

namespace TechFeed.API.Mapping;

/// <summary>
/// Maps the domain <see cref="Article"/> to the public <see cref="ArticleResponse"/>
/// at the API boundary, so domain-only fields are never exposed.
/// </summary>
public static class ArticleResponseMapper
{
    public static ArticleResponse ToResponse(this Article article)
    {
        return new ArticleResponse(
            article.Id,
            article.Source,
            article.Title,
            article.Url,
            article.CoverImage,
            article.Description,
            new List<string>(article.Tags),
            article.Score,
            article.PublishedAt);
    }
}

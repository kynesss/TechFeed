namespace TechFeed.Shared;

/// <summary>
/// Public API representation of an article. Kept separate from the domain
/// model so internal fields (e.g. ExternalId, FetchedAt) are not exposed.
/// </summary>
public record ArticleResponse(
    string Id,
    string Source,
    string Title,
    string Url,
    string? CoverImage,
    string? Description,
    List<string> Tags,
    int Score,
    DateTime PublishedAt);

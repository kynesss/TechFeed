namespace TechFeed.Core;

/// <summary>
/// Domain model for an aggregated article. Deliberately free of any
/// persistence/infrastructure concerns (no MongoDB/BSON attributes).
/// </summary>
public class Article
{
    public string Id { get; set; } = string.Empty;

    public string ExternalId { get; set; } = string.Empty;

    /// <summary>Origin of the article, e.g. "devto" or "hackernews".</summary>
    public string Source { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string? CoverImage { get; set; }

    public string? Description { get; set; }

    public List<string> Tags { get; set; } = new();

    public int Score { get; set; }

    public DateTime PublishedAt { get; set; }

    public DateTime FetchedAt { get; set; }
}

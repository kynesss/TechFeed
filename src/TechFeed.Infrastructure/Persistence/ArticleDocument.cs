using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TechFeed.Infrastructure.Persistence;

/// <summary>
/// MongoDB persistence representation of an article. Kept separate from the
/// domain <see cref="TechFeed.Core.Article"/> so BSON/Mongo details never leak
/// into TechFeed.Core.
/// </summary>
public class ArticleDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("externalId")]
    public string ExternalId { get; set; } = string.Empty;

    [BsonElement("source")]
    public string Source { get; set; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("url")]
    public string Url { get; set; } = string.Empty;

    [BsonElement("coverImage")]
    [BsonIgnoreIfNull]
    public string? CoverImage { get; set; }

    [BsonElement("description")]
    [BsonIgnoreIfNull]
    public string? Description { get; set; }

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();

    [BsonElement("score")]
    public int Score { get; set; }

    [BsonElement("publishedAt")]
    public DateTime PublishedAt { get; set; }

    [BsonElement("fetchedAt")]
    public DateTime FetchedAt { get; set; }
}

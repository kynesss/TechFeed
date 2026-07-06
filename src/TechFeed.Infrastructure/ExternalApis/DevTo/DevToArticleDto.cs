using System.Text.Json.Serialization;

namespace TechFeed.Infrastructure.ExternalApis.DevTo;

public class DevToArticleDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("cover_image")]
    public string? CoverImage { get; set; }

    [JsonPropertyName("published_at")]
    public DateTime PublishedAt { get; set; }

    [JsonPropertyName("tag_list")]
    public List<string> TagList { get; set; } = new();

    [JsonPropertyName("positive_reactions_count")]
    public int PositiveReactionsCount { get; set; }
}

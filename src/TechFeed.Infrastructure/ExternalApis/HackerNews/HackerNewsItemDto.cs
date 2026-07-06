using System.Text.Json.Serialization;

namespace TechFeed.Infrastructure.ExternalApis.HackerNews;

public class HackerNewsItemDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("score")]
    public int Score { get; set; }

    /// <summary>Creation time as a Unix timestamp in seconds.</summary>
    [JsonPropertyName("time")]
    public long Time { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    public DateTime PublishedAt => DateTimeOffset.FromUnixTimeSeconds(Time).UtcDateTime;
}

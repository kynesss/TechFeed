namespace TechFeed.Infrastructure.Configuration;

/// <summary>
/// Bound from the "RedisSettings" section of appsettings.json.
/// </summary>
public class RedisSettings
{
    public string ConnectionString { get; set; } = string.Empty;
}

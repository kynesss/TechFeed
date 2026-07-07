namespace TechFeed.API.Configuration;

/// <summary>
/// Bound from the "ApiSettings" section of appsettings.json.
/// </summary>
public class ApiSettings
{
    public string RefreshApiKey { get; set; } = string.Empty;
}

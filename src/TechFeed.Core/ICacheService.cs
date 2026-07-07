namespace TechFeed.Core;

/// <summary>
/// Abstraction over a distributed cache. Core defines the contract; the
/// concrete Redis implementation lives in TechFeed.Infrastructure.
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);

    Task SetAsync<T>(string key, T value, TimeSpan ttl);

    Task DeleteByPatternAsync(string pattern);

    Task DeleteAsync(string key);
}

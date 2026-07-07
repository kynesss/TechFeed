using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using TechFeed.Core;

namespace TechFeed.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _multiplexer;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IConnectionMultiplexer multiplexer, ILogger<RedisCacheService> logger)
    {
        _multiplexer = multiplexer;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var db = _multiplexer.GetDatabase();
        var value = await db.StringGetAsync(key);

        if (value.IsNullOrEmpty)
        {
            _logger.LogDebug("Cache MISS for key {Key}", key);
            return default;
        }

        _logger.LogDebug("Cache HIT for key {Key}", key);
        return JsonSerializer.Deserialize<T>(value.ToString());
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
    {
        var db = _multiplexer.GetDatabase();
        var json = JsonSerializer.Serialize(value);

        await db.StringSetAsync(key, json, ttl);
        _logger.LogDebug("Cache SET for key {Key} (ttl {Ttl})", key, ttl);
    }

    public async Task DeleteByPatternAsync(string pattern)
    {
        var db = _multiplexer.GetDatabase();
        var deleted = 0;

        // Keys(pattern) is SCAN-based (non-blocking), unlike the raw KEYS command.
        foreach (var endpoint in _multiplexer.GetEndPoints())
        {
            var server = _multiplexer.GetServer(endpoint);

            // Only primaries hold the authoritative keyspace for deletion.
            if (server.IsReplica)
            {
                continue;
            }

            foreach (var key in server.Keys(pattern: pattern))
            {
                await db.KeyDeleteAsync(key);
                deleted++;
            }
        }

        _logger.LogInformation(
            "Cache invalidation for pattern {Pattern} removed {Count} key(s)", pattern, deleted);
    }

    public async Task DeleteAsync(string key)
    {
        var db = _multiplexer.GetDatabase();
        await db.KeyDeleteAsync(key);
        _logger.LogDebug("Cache DELETE for key {Key}", key);
    }
}

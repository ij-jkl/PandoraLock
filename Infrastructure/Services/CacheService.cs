using Application.Common.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly TimeSpan _defaultExpiration;

    public CacheService(IConnectionMultiplexer redis, TimeSpan? defaultExpiration = null)
    {
        _redis = redis;
        _database = _redis.GetDatabase();
        _defaultExpiration = defaultExpiration ?? TimeSpan.FromMinutes(60);
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        var value = await _database.StringGetAsync(key);

        if (!value.HasValue)
        {
            return null;
        }

        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var serializedValue = JsonSerializer.Serialize(value);
        var expirationTime = expiration ?? _defaultExpiration;

        await _database.StringSetAsync(key, serializedValue, expirationTime);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _database.KeyDeleteAsync(key);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _database.KeyExistsAsync(key);
    }
}

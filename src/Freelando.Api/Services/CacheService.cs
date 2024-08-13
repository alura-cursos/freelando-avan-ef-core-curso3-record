
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Freelando.Api.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache cache;

    public CacheService(IDistributedCache cache)
    {
        this.cache = cache;
    }

    public async Task<T> GetCachedDataAsync<T>(string key)
    {
        var cachedData = await cache.GetStringAsync(key);
        return cachedData != null ? JsonSerializer.Deserialize<T>(cachedData) : default;

    }

    public async Task RemoveCachedDataAsync(string key)
    {
        await cache.RemoveAsync(key);
    }

    public async Task SetCachedDataAsync<T>(string key, T data, TimeSpan expiration)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };

        var serializedData = JsonSerializer.Serialize(data);
        await cache.SetStringAsync(key, serializedData, options);

    }
}

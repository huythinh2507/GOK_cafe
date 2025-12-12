using GOKCafe.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace GOKCafe.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private static readonly HashSet<string> _cacheKeys = new();
    private static readonly object _lock = new();

    public CacheService(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        var cachedValue = await _distributedCache.GetStringAsync(key, cancellationToken);

        if (string.IsNullOrEmpty(cachedValue))
            return null;

        return JsonSerializer.Deserialize<T>(cachedValue);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30)
        };

        var serializedValue = JsonSerializer.Serialize(value);
        await _distributedCache.SetStringAsync(key, serializedValue, options, cancellationToken);

        lock (_lock)
        {
            _cacheKeys.Add(key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _distributedCache.RemoveAsync(key, cancellationToken);

        lock (_lock)
        {
            _cacheKeys.Remove(key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        List<string> keysToRemove;

        lock (_lock)
        {
            keysToRemove = _cacheKeys.Where(k => k.StartsWith(prefix)).ToList();
        }

        var tasks = keysToRemove.Select(key => RemoveAsync(key, cancellationToken));
        await Task.WhenAll(tasks);
    }
}

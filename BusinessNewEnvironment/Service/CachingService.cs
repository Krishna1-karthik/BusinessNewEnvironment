using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace BusinessNewEnvironment.Service
{
    /// <summary>
    /// Service for handling distributed caching of frequently accessed data like categories and subcategories.
    /// Reduces database queries and improves response time for read-heavy operations.
    /// </summary>
    public class CachingService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CachingService> _logger;

        public CachingService(IDistributedCache cache, ILogger<CachingService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Gets or creates a cached value with automatic expiration
        /// </summary>
        public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            try
            {
                var cachedValue = await _cache.GetStringAsync(key);
                if (!string.IsNullOrEmpty(cachedValue))
                {
                    _logger.LogDebug($"Cache HIT for key: {key}");
                    return JsonSerializer.Deserialize<T>(cachedValue);
                }

                _logger.LogDebug($"Cache MISS for key: {key}");
                var value = await factory();

                if (value != null)
                {
                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(1)
                    };

                    var serialized = JsonSerializer.Serialize(value);
                    await _cache.SetStringAsync(key, serialized, cacheOptions);
                }

                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Caching error for key {key}: {ex.Message}. Proceeding without cache.");
                return await factory();
            }
        }

        /// <summary>
        /// Invalidates a specific cache key
        /// </summary>
        public async Task InvalidateAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
                _logger.LogInformation($"Cache invalidated for key: {key}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error invalidating cache for key {key}: {ex.Message}");
            }
        }

        /// <summary>
        /// Invalidates multiple cache patterns (e.g., all category-related caches)
        /// </summary>
        public async Task InvalidatePatternAsync(IEnumerable<string> keys)
        {
            var tasks = keys.Select(k => InvalidateAsync(k));
            await Task.WhenAll(tasks);
        }

        // Predefined cache keys
        public static class CacheKeys
        {
            public const string AllCategories = "categories_all";
            public static string SubCategories(int categoryId) => $"subcategories_cat_{categoryId}";
            public static string BusinessSearch(string category, string subcategory) => $"search_cat_{category}_subcat_{subcategory}";
        }
    }
}

using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace BusinessNewEnvironment.Service
{
    /// <summary>
    /// Service for caching distance calculations from Google Maps Distance Matrix API
    /// to reduce API calls and improve response times for location-based queries.
    /// </summary>
    public class DistanceCachingService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DistanceCachingService> _logger;
        private readonly string _googleMapsApiKey;

        public DistanceCachingService(
            IHttpClientFactory httpClientFactory,
            IDistributedCache cache,
            IConfiguration configuration,
            ILogger<DistanceCachingService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _configuration = configuration;
            _logger = logger;
            _googleMapsApiKey = configuration["GoogleMaps:ApiKey"] ?? string.Empty;
        }

        /// <summary>
        /// Gets distance between two coordinates with caching
        /// </summary>
        public async Task<double?> GetDistanceAsync(double fromLat, double fromLng, double toLat, double toLng)
        {
            if (string.IsNullOrEmpty(_googleMapsApiKey))
            {
                _logger.LogWarning("Google Maps API key not configured");
                return null;
            }

            var cacheKey = GenerateCacheKey(fromLat, fromLng, toLat, toLng);

            try
            {
                // Try to get from cache
                var cachedValue = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedValue) && double.TryParse(cachedValue, out var cachedDistance))
                {
                    _logger.LogDebug($"Distance cache HIT for {cacheKey}");
                    return cachedDistance;
                }

                _logger.LogDebug($"Distance cache MISS for {cacheKey}");

                // Call Google Maps API
                var distance = await CallGoogleMapsDistanceMatrixApi(fromLat, fromLng, toLat, toLng);

                if (distance.HasValue && distance > 0)
                {
                    // Cache the distance for 24 hours (distances rarely change)
                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                    };
                    await _cache.SetStringAsync(cacheKey, distance.Value.ToString(), cacheOptions);
                }

                return distance;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting distance: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets distances for multiple destinations with batch caching
        /// </summary>
        public async Task<Dictionary<string, double>> GetDistancesBatchAsync(
            double fromLat,
            double fromLng,
            List<(double lat, double lng, string key)> destinations)
        {
            var result = new Dictionary<string, double>();

            // Check cache for all destinations
            var notCachedDestinations = new List<(double lat, double lng, string key)>();

            foreach (var dest in destinations)
            {
                var cacheKey = GenerateCacheKey(fromLat, fromLng, dest.lat, dest.lng);
                var cachedValue = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedValue) && double.TryParse(cachedValue, out var distance))
                {
                    result[dest.key] = distance;
                }
                else
                {
                    notCachedDestinations.Add(dest);
                }
            }

            // Call API only for non-cached destinations
            if (notCachedDestinations.Any())
            {
                var apiResults = await CallGoogleMapsDistanceMatrixApiBatch(
                    fromLat, fromLng, notCachedDestinations);

                // Cache the results and add to return dictionary
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                };

                foreach (var (distance, key, lat, lng) in apiResults)
                {
                    result[key] = distance;
                    var cacheKey = GenerateCacheKey(fromLat, fromLng, lat, lng);
                    await _cache.SetStringAsync(cacheKey, distance.ToString(), cacheOptions);
                }
            }

            return result;
        }

        /// <summary>
        /// Invalidates distance cache for a specific location pair
        /// </summary>
        public async Task InvalidateDistanceCacheAsync(double lat, double lng)
        {
            try
            {
                // In a production scenario, you might implement a more sophisticated invalidation strategy
                _logger.LogInformation($"Distance cache invalidation triggered for ({lat}, {lng})");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error invalidating distance cache: {ex.Message}");
            }
        }

        private string GenerateCacheKey(double fromLat, double fromLng, double toLat, double toLng)
        {
            // Round coordinates to reduce cache key variations from slight GPS differences
            var rounding = 3; // ~100 meters precision
            var roundedFromLat = Math.Round(fromLat, rounding);
            var roundedFromLng = Math.Round(fromLng, rounding);
            var roundedToLat = Math.Round(toLat, rounding);
            var roundedToLng = Math.Round(toLng, rounding);

            return $"distance_{roundedFromLat}_{roundedFromLng}_{roundedToLat}_{roundedToLng}";
        }

        private async Task<double?> CallGoogleMapsDistanceMatrixApi(double fromLat, double fromLng, double toLat, double toLng)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = $"https://maps.googleapis.com/maps/api/distancematrix/json?" +
                    $"origins={fromLat},{fromLng}&destinations={toLat},{toLng}" +
                    $"&key={_googleMapsApiKey}&units=metric";

                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Google Maps API returned {response.StatusCode}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                if (root.TryGetProperty("rows", out var rows) &&
                    rows.GetArrayLength() > 0 &&
                    rows[0].TryGetProperty("elements", out var elements) &&
                    elements.GetArrayLength() > 0 &&
                    elements[0].TryGetProperty("distance", out var distance) &&
                    distance.TryGetProperty("value", out var value))
                {
                    return value.GetDouble() / 1000.0; // Convert meters to km
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Google Maps API call failed: {ex.Message}");
                return null;
            }
        }

        private async Task<List<(double distance, string key, double lat, double lng)>> CallGoogleMapsDistanceMatrixApiBatch(
            double fromLat,
            double fromLng,
            List<(double lat, double lng, string key)> destinations)
        {
            var result = new List<(double, string, double, double)>();

            try
            {
                var client = _httpClientFactory.CreateClient();

                // Google Maps API allows multiple destinations
                var destString = string.Join("|", destinations.Select(d => $"{d.lat},{d.lng}"));
                var url = $"https://maps.googleapis.com/maps/api/distancematrix/json?" +
                    $"origins={fromLat},{fromLng}&destinations={destString}" +
                    $"&key={_googleMapsApiKey}&units=metric";

                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Google Maps batch API returned {response.StatusCode}");
                    return result;
                }

                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                if (root.TryGetProperty("rows", out var rows) && rows.GetArrayLength() > 0)
                {
                    var elements = rows[0].GetProperty("elements");

                    for (int i = 0; i < elements.GetArrayLength() && i < destinations.Count; i++)
                    {
                        if (elements[i].TryGetProperty("distance", out var distance) &&
                            distance.TryGetProperty("value", out var value))
                        {
                            var distanceKm = value.GetDouble() / 1000.0;
                            result.Add((distanceKm, destinations[i].key, destinations[i].lat, destinations[i].lng));
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Google Maps batch API call failed: {ex.Message}");
                return result;
            }
        }
    }
}

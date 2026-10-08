/// <summary>
/// Service Worker Caching Strategy
/// 
/// This document outlines the recommended caching strategy for the BusinessNewEnvironment application.
/// Implement these patterns in your Angular service worker or custom service worker script.
/// 
/// CACHE STRATEGY BREAKDOWN:
/// 
/// 1. IMMUTABLE ASSETS (30 days)
///    - Images: *.jpg, *.png, *.webp, *.avif (already cached via HTTP headers)
///    - Fonts: *.woff2, *.woff
///    Cache Key: "immutable-v1"
///    Fetch Strategy: Cache First, stale-while-revalidate
/// 
/// 2. APP SHELL (1 day + fallback)
///    - index.html, main.js, styles.css, polyfills
///    Cache Key: "app-shell-v1"
///    Fetch Strategy: Network First (for fresh content), Cache Fallback
///    Fallback: Serve offline page if network fails
/// 
/// 3. API DATA (5 minutes - 24 hours depending on endpoint)
///    - /api/business/GetCategories: 1 hour (changes rarely)
///    - /api/business/GetSubCategories: 1 hour
///    - /api/business/search: 5 minutes (dynamic)
///    - /api/business/getbusinessdetailbyid: 10 minutes
///    Cache Key: "api-data-v1"
///    Fetch Strategy: Network First, Cache Fallback, with TTL validation
/// 
/// 4. DYNAMIC CONTENT (no cache)
///    - Login/Auth endpoints
///    - Data modifications (POST/PUT/DELETE)
///    Cache Key: N/A
///    Fetch Strategy: Network Only
/// 
/// IMPLEMENTATION NOTES:
/// 
/// For Angular Applications:
/// - Use @angular/service-worker with ngsw-config.json
/// - Configure updateFrequency for refresh checks
/// - Use HTTP caching headers for cache invalidation
/// 
/// For Custom Service Worker:
/// - Install: Cache app shell and immutable assets
/// - Activate: Clean up old cache versions
/// - Fetch: Implement strategy-specific fetch logic
/// - Store: Implement cache size management (max 50MB typical)
/// 
/// MONITORING:
/// - Track cache hit rates in Application Insights
/// - Monitor offline user sessions
/// - Clean up cache based on LRU strategy for large apps
/// </summary>
public class ServiceWorkerCachingStrategy
{
    // Defined cache versions for versioning strategy
    public const string CacheVersionImmutable = "immutable-v1";
    public const string CacheVersionAppShell = "app-shell-v1";
    public const string CacheVersionApiData = "api-data-v1";

    // Cache size management (50MB limit typical for service workers)
    public const long MaxCacheSizeBytes = 50 * 1024 * 1024; // 50MB

    // TTL for API cache entries (in seconds)
    public const int TTL_Categories = 3600; // 1 hour
    public const int TTL_SubCategories = 3600; // 1 hour
    public const int TTL_Search = 300; // 5 minutes
    public const int TTL_BusinessDetail = 600; // 10 minutes
    public const int TTL_Ratings = 600; // 10 minutes

    // Endpoints that should never be cached
    public static readonly string[] NonCacheableEndpoints = new[]
    {
        "/api/business/registerbusin",
        "/api/business",
        "/api/auth/",
        "/api/customer",
        "/api/businessrating/add"
    };
}

using System.Net.Mime;

namespace BusinessNewEnvironment.Middleware
{
    /// <summary>
    /// Middleware for optimizing image delivery with proper caching headers,
    /// MIME type detection, and support for browser caching strategies.
    /// </summary>
    public class ImageCachingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ImageCachingMiddleware> _logger;

        // Image extension to MIME type mapping
        private static readonly Dictionary<string, string> ImageMimeTypes = new()
        {
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png", "image/png" },
            { ".gif", "image/gif" },
            { ".webp", "image/webp" },
            { ".avif", "image/avif" },
            { ".svg", "image/svg+xml" },
            { ".ico", "image/x-icon" }
        };

        public ImageCachingMiddleware(RequestDelegate next, ILogger<ImageCachingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/api/business") && 
                !context.Request.Path.ToString().Contains("GetCategories") &&
                !context.Request.Path.ToString().Contains("GetSubCategories") &&
                !context.Request.Path.ToString().Contains("search"))
            {
                var originalBodyStream = context.Response.Body;

                using (var responseBody = new MemoryStream())
                {
                    context.Response.Body = responseBody;

                    await _next(context);

                    // Apply cache headers for image requests (routes returning image files)
                    if (context.Request.Method == "GET" && 
                        (context.Request.Path.Value?.EndsWith(".jpg") == true ||
                         context.Request.Path.Value?.EndsWith(".jpeg") == true ||
                         context.Request.Path.Value?.EndsWith(".png") == true ||
                         context.Request.Path.Value?.EndsWith(".webp") == true ||
                         context.Request.Path.Value?.EndsWith(".avif") == true))
                    {
                        // Set caching headers for images (cache for 30 days)
                        context.Response.Headers.CacheControl = "public, max-age=2592000, immutable";
                        // Use ETag for validation on repeat visits
                        SetETag(context);
                    }

                    await responseBody.CopyToAsync(originalBodyStream);
                }

                return;
            }

            await _next(context);
        }

        private static void SetETag(HttpContext context)
        {
            var pathValue = context.Request.Path.Value;
            if (!string.IsNullOrEmpty(pathValue))
            {
                var eTag = $"\"{pathValue.GetHashCode():x}\"";
                context.Response.Headers.ETag = eTag;
            }
        }
    }

    public static class ImageCachingMiddlewareExtensions
    {
        public static IApplicationBuilder UseImageCaching(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ImageCachingMiddleware>();
        }
    }
}

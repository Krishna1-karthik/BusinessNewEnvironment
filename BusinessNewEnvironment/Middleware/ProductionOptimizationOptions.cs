namespace BusinessNewEnvironment.Middleware
{
    /// <summary>
    /// Configuration class for production optimization settings
    /// </summary>
    public class ProductionOptimizationOptions
    {
        /// <summary>
        /// Enable response compression (gzip, brotli, deflate)
        /// </summary>
        public bool EnableCompression { get; set; } = true;

        /// <summary>
        /// Minimum size threshold for compression (in bytes)
        /// </summary>
        public int CompressionMinSizeBytes { get; set; } = 512;

        /// <summary>
        /// Enable ETag generation for static content
        /// </summary>
        public bool EnableETags { get; set; } = true;

        /// <summary>
        /// Maximum age for cache headers (in seconds)
        /// </summary>
        public int CacheMaxAgeDays { get; set; } = 30;
    }
}

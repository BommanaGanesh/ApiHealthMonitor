namespace ApiHealthMonitor.Configuration;

/// <summary>
/// Configuration settings for health check operations.
/// </summary>
public class HealthCheckOptions
{
    public const string SectionName = "HealthCheck";

    /// <summary>
    /// The timeout duration for HTTP requests in seconds. Default: 10.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// The HTTP User-Agent header value. Default: ApiHealthMonitor/1.0.
    /// </summary>
    public string UserAgent { get; set; } = "ApiHealthMonitor/1.0";

    /// <summary>
    /// If true, rejects requests to localhost and private IP addresses (127.0.0.1, 10.x.x.x, 172.16-31.x.x, 192.168.x.x).
    /// Default: false (allow all, but should be enabled in production for SSRF protection).
    /// </summary>
    public bool BlockPrivateNetworks { get; set; } = false;

    /// <summary>
    /// Maximum response size to read in bytes. Default: 10 MB.
    /// </summary>
    public int MaxResponseSizeBytes { get; set; } = 10 * 1024 * 1024;
}

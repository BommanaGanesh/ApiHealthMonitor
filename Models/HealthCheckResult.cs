namespace ApiHealthMonitor.Models;

/// <summary>
/// Represents the result of a health check for an external API.
/// </summary>
public class HealthCheckResult
{
    /// <summary>
    /// The URL that was checked.
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// Indicates whether the API is healthy (2xx status code).
    /// </summary>
    public required bool IsHealthy { get; init; }

    /// <summary>
    /// The HTTP status code returned by the external API, if available.
    /// </summary>
    public int? StatusCode { get; init; }

    /// <summary>
    /// The response time in milliseconds.
    /// </summary>
    public required long ResponseTimeMs { get; init; }

    /// <summary>
    /// The UTC timestamp when the check was performed.
    /// </summary>
    public required DateTime CheckedAt { get; init; }

    /// <summary>
    /// Error message if the check failed, or null if successful.
    /// </summary>
    public string? Error { get; init; }
}

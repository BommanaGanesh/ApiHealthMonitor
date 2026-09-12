using ApiHealthMonitor.Models;

namespace ApiHealthMonitor.Services;

/// <summary>
/// Service interface for performing health checks on external APIs.
/// </summary>
public interface IHealthCheckService
{
    /// <summary>
    /// Checks the health of an external API by sending an HTTP GET request.
    /// </summary>
    /// <param name="url">The absolute URL of the API to check.</param>
    /// <param name="cancellationToken">Cancellation token to support graceful shutdown.</param>
    /// <returns>A HealthCheckResult containing the check outcome.</returns>
    Task<HealthCheckResult> CheckHealthAsync(string url, CancellationToken cancellationToken = default);
}

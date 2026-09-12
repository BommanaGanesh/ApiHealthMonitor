using ApiHealthMonitor.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiHealthMonitor.Controllers;

/// <summary>
/// API controller for health checking external services.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IHealthCheckService _healthCheckService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(IHealthCheckService healthCheckService, ILogger<HealthController> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    /// <summary>
    /// Checks the health and availability of an external HTTP/HTTPS API.
    /// </summary>
    /// <param name="url">The absolute URL of the API to check (e.g., https://example.com).</param>
    /// <param name="cancellationToken">Cancellation token for request cancellation.</param>
    /// <returns>A HealthCheckResult containing the health status, response time, and any error information.</returns>
    [HttpGet("check")]
    public async Task<IActionResult> CheckHealth([FromQuery] string? url, CancellationToken cancellationToken)
    {
        // Validate that URL is provided
        if (string.IsNullOrWhiteSpace(url))
        {
            _logger.LogWarning("Health check endpoint called without URL parameter.");
            return BadRequest(new { error = "URL parameter is required." });
        }

        // Call the service to perform the health check
        var result = await _healthCheckService.CheckHealthAsync(url, cancellationToken);

        // If validation failed, return 400 with error details
        if (!string.IsNullOrEmpty(result.Error) && result.StatusCode == null && result.ResponseTimeMs == 0)
        {
            _logger.LogWarning("Returning 400 Bad Request for URL: {Url}. Error: {Error}", url, result.Error);
            return BadRequest(new { error = result.Error });
        }

        // Return 200 with the health check result (regardless of whether the target API is healthy)
        return Ok(result);
    }
}

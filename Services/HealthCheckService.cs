using System.Diagnostics;
using ApiHealthMonitor.Configuration;
using ApiHealthMonitor.Models;
using Microsoft.Extensions.Options;

namespace ApiHealthMonitor.Services;

/// <summary>
/// Service for performing health checks on external APIs.
/// </summary>
public class HealthCheckService : IHealthCheckService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HealthCheckService> _logger;
    private readonly HealthCheckOptions _options;

    public HealthCheckService(
        HttpClient httpClient,
        ILogger<HealthCheckService> logger,
        IOptions<HealthCheckOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(string url, CancellationToken cancellationToken = default)
    {
        // Validate the URL
        var validationError = UrlValidator.ValidateUrl(url, _options.BlockPrivateNetworks);
        if (validationError != null)
        {
            _logger.LogWarning("Health check validation failed for URL: {Url}. Reason: {Reason}", url, validationError);
            return new HealthCheckResult
            {
                Url = url,
                IsHealthy = false,
                StatusCode = null,
                ResponseTimeMs = 0,
                CheckedAt = DateTime.UtcNow,
                Error = validationError
            };
        }

        _logger.LogInformation("Starting health check for URL: {Url}", url);

        var stopwatch = Stopwatch.StartNew();
        var responseTimeMs = 0L;
        int? statusCode = null;
        string? error = null;
        bool isHealthy = false;

        try
        {
            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            stopwatch.Stop();
            responseTimeMs = stopwatch.ElapsedMilliseconds;
            statusCode = (int)response.StatusCode;

            isHealthy = (int)response.StatusCode >= 200 && (int)response.StatusCode < 300;

            _logger.LogInformation(
                "Health check completed for {Url}. StatusCode: {StatusCode}, ResponseTime: {ResponseTimeMs}ms, IsHealthy: {IsHealthy}",
                url,
                statusCode,
                responseTimeMs,
                isHealthy);

            return new HealthCheckResult
            {
                Url = url,
                IsHealthy = isHealthy,
                StatusCode = statusCode,
                ResponseTimeMs = responseTimeMs,
                CheckedAt = DateTime.UtcNow,
                Error = isHealthy ? null : $"HTTP {statusCode}: Unhealthy response."
            };
        }
        catch (OperationCanceledException ex)
        {
            stopwatch.Stop();
            responseTimeMs = stopwatch.ElapsedMilliseconds;
            error = "Request timed out.";
            _logger.LogWarning(ex, "Health check for {Url} was cancelled or timed out. ResponseTime: {ResponseTimeMs}ms", url, responseTimeMs);
            return new HealthCheckResult
            {
                Url = url,
                IsHealthy = false,
                StatusCode = null,
                ResponseTimeMs = responseTimeMs,
                CheckedAt = DateTime.UtcNow,
                Error = error
            };
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            responseTimeMs = stopwatch.ElapsedMilliseconds;
            error = "Network error or invalid address.";
            _logger.LogWarning(ex, "Health check for {Url} encountered a network error. ResponseTime: {ResponseTimeMs}ms", url, responseTimeMs);
            return new HealthCheckResult
            {
                Url = url,
                IsHealthy = false,
                StatusCode = null,
                ResponseTimeMs = responseTimeMs,
                CheckedAt = DateTime.UtcNow,
                Error = error
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            responseTimeMs = stopwatch.ElapsedMilliseconds;
            error = "An unexpected error occurred.";
            _logger.LogError(ex, "Unexpected error during health check for {Url}. ResponseTime: {ResponseTimeMs}ms", url, responseTimeMs);
            return new HealthCheckResult
            {
                Url = url,
                IsHealthy = false,
                StatusCode = null,
                ResponseTimeMs = responseTimeMs,
                CheckedAt = DateTime.UtcNow,
                Error = error
            };
        }
    }
}

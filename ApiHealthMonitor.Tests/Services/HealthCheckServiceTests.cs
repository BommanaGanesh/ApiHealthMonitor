using ApiHealthMonitor.Configuration;
using ApiHealthMonitor.Models;
using ApiHealthMonitor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ApiHealthMonitor.Tests.Services;

public class HealthCheckServiceTests
{
    private readonly Mock<ILogger<HealthCheckService>> _mockLogger;
    private readonly HealthCheckOptions _defaultOptions;
    private readonly IOptions<HealthCheckOptions> _options;
    private readonly HealthCheckService _service;
    private readonly HttpClientHandler _httpClientHandler;
    private readonly HttpClient _httpClient;

    public HealthCheckServiceTests()
    {
        _mockLogger = new Mock<ILogger<HealthCheckService>>();
        _defaultOptions = new HealthCheckOptions
        {
            TimeoutSeconds = 10,
            UserAgent = "ApiHealthMonitor/1.0",
            BlockPrivateNetworks = false
        };
        _options = Options.Create(_defaultOptions);

        // Create HttpClient with a real handler for these tests
        _httpClientHandler = new HttpClientHandler();
        _httpClient = new HttpClient(_httpClientHandler);

        _service = new HealthCheckService(_httpClient, _mockLogger.Object, _options);
    }

    [Fact]
    public async Task CheckHealthAsync_WithValidUrl_ReturnsHealthyResult()
    {
        // Arrange
        var url = "https://www.google.com";

        // Act
        var result = await _service.CheckHealthAsync(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(url, result.Url);
        Assert.NotNull(result.StatusCode);
        Assert.True(result.ResponseTimeMs >= 0);
        Assert.NotEqual(default(DateTime), result.CheckedAt);
    }

    [Fact]
    public async Task CheckHealthAsync_WithInvalidUrl_ReturnsBadRequestResult()
    {
        // Arrange
        var url = "invalid-url";

        // Act
        var result = await _service.CheckHealthAsync(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(url, result.Url);
        Assert.False(result.IsHealthy);
        Assert.Null(result.StatusCode);
        Assert.Equal(0, result.ResponseTimeMs);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task CheckHealthAsync_WithEmptyUrl_ReturnsError()
    {
        // Arrange
        var url = "";

        // Act
        var result = await _service.CheckHealthAsync(url);

        // Assert
        Assert.False(result.IsHealthy);
        Assert.NotNull(result.Error);
        Assert.Null(result.StatusCode);
    }

    [Fact]
    public async Task CheckHealthAsync_WithNullUrl_ReturnsError()
    {
        // Arrange
        string? url = null;

        // Act
        var result = await _service.CheckHealthAsync(url ?? string.Empty);

        // Assert
        Assert.False(result.IsHealthy);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task CheckHealthAsync_WithUnsupportedScheme_ReturnsError()
    {
        // Arrange
        var url = "ftp://files.example.com";

        // Act
        var result = await _service.CheckHealthAsync(url);

        // Assert
        Assert.False(result.IsHealthy);
        Assert.NotNull(result.Error);
        Assert.Null(result.StatusCode);
    }

    [Fact]
    public async Task CheckHealthAsync_With2xxResponse_ReturnsHealthy()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(System.Net.HttpStatusCode.OK, "Success");
        var httpClient = new HttpClient(mockHandler);
        var service = new HealthCheckService(httpClient, _mockLogger.Object, _options);
        var url = "https://example.com";

        // Act
        var result = await service.CheckHealthAsync(url);

        // Assert
        Assert.True(result.IsHealthy);
        Assert.Equal(200, result.StatusCode);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task CheckHealthAsync_With201Response_ReturnsHealthy()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(System.Net.HttpStatusCode.Created, "Created");
        var httpClient = new HttpClient(mockHandler);
        var service = new HealthCheckService(httpClient, _mockLogger.Object, _options);
        var url = "https://example.com";

        // Act
        var result = await service.CheckHealthAsync(url);

        // Assert
        Assert.True(result.IsHealthy);
        Assert.Equal(201, result.StatusCode);
    }

    [Fact]
    public async Task CheckHealthAsync_With204Response_ReturnsHealthy()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(System.Net.HttpStatusCode.NoContent, string.Empty);
        var httpClient = new HttpClient(mockHandler);
        var service = new HealthCheckService(httpClient, _mockLogger.Object, _options);
        var url = "https://example.com";

        // Act
        var result = await service.CheckHealthAsync(url);

        // Assert
        Assert.True(result.IsHealthy);
        Assert.Equal(204, result.StatusCode);
    }

    [Fact]
    public async Task CheckHealthAsync_With404Response_ReturnsUnhealthy()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(System.Net.HttpStatusCode.NotFound, "Not Found");
        var httpClient = new HttpClient(mockHandler);
        var service = new HealthCheckService(httpClient, _mockLogger.Object, _options);
        var url = "https://example.com";

        // Act
        var result = await service.CheckHealthAsync(url);

        // Assert
        Assert.False(result.IsHealthy);
        Assert.Equal(404, result.StatusCode);
        Assert.NotNull(result.Error);
        Assert.Contains("404", result.Error);
    }

    [Fact]
    public async Task CheckHealthAsync_With500Response_ReturnsUnhealthy()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(System.Net.HttpStatusCode.InternalServerError, "Server Error");
        var httpClient = new HttpClient(mockHandler);
        var service = new HealthCheckService(httpClient, _mockLogger.Object, _options);
        var url = "https://example.com";

        // Act
        var result = await service.CheckHealthAsync(url);

        // Assert
        Assert.False(result.IsHealthy);
        Assert.Equal(500, result.StatusCode);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task CheckHealthAsync_WithTimeout_ReturnsTimeoutError()
    {
        // Arrange
        var mockHandler = new TimeoutHttpMessageHandler();
        var httpClient = new HttpClient(mockHandler);
        var service = new HealthCheckService(httpClient, _mockLogger.Object, _options);
        var url = "https://slow-example.com";

        // Act
        var result = await service.CheckHealthAsync(url);

        // Assert
        Assert.False(result.IsHealthy);
        Assert.Null(result.StatusCode);
        Assert.NotNull(result.Error);
        Assert.Equal("Request timed out.", result.Error);
    }

    [Fact]
    public async Task CheckHealthAsync_WithNetworkError_ReturnsNetworkError()
    {
        // Arrange
        var mockHandler = new NetworkErrorHttpMessageHandler();
        var httpClient = new HttpClient(mockHandler);
        var service = new HealthCheckService(httpClient, _mockLogger.Object, _options);
        var url = "https://unreachable.example.com";

        // Act
        var result = await service.CheckHealthAsync(url);

        // Assert
        Assert.False(result.IsHealthy);
        Assert.Null(result.StatusCode);
        Assert.NotNull(result.Error);
        Assert.Equal("Network error or invalid address.", result.Error);
    }

    [Fact]
    public async Task CheckHealthAsync_RecordsResponseTime()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(System.Net.HttpStatusCode.OK, "Success");
        var httpClient = new HttpClient(mockHandler);
        var service = new HealthCheckService(httpClient, _mockLogger.Object, _options);
        var url = "https://example.com";

        // Act
        var result = await service.CheckHealthAsync(url);

        // Assert
        Assert.True(result.ResponseTimeMs >= 0);
    }

    [Fact]
    public async Task CheckHealthAsync_SetsCheckedAtTimestamp()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(System.Net.HttpStatusCode.OK, "Success");
        var httpClient = new HttpClient(mockHandler);
        var service = new HealthCheckService(httpClient, _mockLogger.Object, _options);
        var url = "https://example.com";
        var beforeCheck = DateTime.UtcNow;

        // Act
        var result = await service.CheckHealthAsync(url);

        // Assert
        var afterCheck = DateTime.UtcNow;
        Assert.True(result.CheckedAt >= beforeCheck);
        Assert.True(result.CheckedAt <= afterCheck);
    }

    [Fact]
    public async Task CheckHealthAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var mockHandler = new SlowHttpMessageHandler();
        var httpClient = new HttpClient(mockHandler);
        var service = new HealthCheckService(httpClient, _mockLogger.Object, _options);
        var url = "https://slow.example.com";
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));

        // Act
        var result = await service.CheckHealthAsync(url, cts.Token);

        // Assert
        Assert.False(result.IsHealthy);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task CheckHealthAsync_LogsHealthCheckStarted()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(System.Net.HttpStatusCode.OK, "Success");
        var httpClient = new HttpClient(mockHandler);
        var service = new HealthCheckService(httpClient, _mockLogger.Object, _options);
        var url = "https://example.com";

        // Act
        await service.CheckHealthAsync(url);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting health check")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CheckHealthAsync_LogsValidationFailure()
    {
        // Arrange
        var url = "invalid";

        // Act
        await _service.CheckHealthAsync(url);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("validation failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // Helper classes for mocking HTTP responses
    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly System.Net.HttpStatusCode _statusCode;
        private readonly string _content;

        public MockHttpMessageHandler(System.Net.HttpStatusCode statusCode, string content)
        {
            _statusCode = statusCode;
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_content)
            };
            return Task.FromResult(response);
        }
    }

    private class TimeoutHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new OperationCanceledException("Request timeout");
        }
    }

    private class NetworkErrorHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new HttpRequestException("Network error");
        }
    }

    private class SlowHttpMessageHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(5000, cancellationToken);
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }
    }
}

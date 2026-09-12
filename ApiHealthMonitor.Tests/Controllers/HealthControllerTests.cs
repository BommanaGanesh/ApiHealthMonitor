using ApiHealthMonitor.Controllers;
using ApiHealthMonitor.Models;
using ApiHealthMonitor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ApiHealthMonitor.Tests.Controllers;

public class HealthControllerTests
{
    private readonly Mock<IHealthCheckService> _mockHealthCheckService;
    private readonly Mock<ILogger<HealthController>> _mockLogger;
    private readonly HealthController _controller;

    public HealthControllerTests()
    {
        _mockHealthCheckService = new Mock<IHealthCheckService>();
        _mockLogger = new Mock<ILogger<HealthController>>();
        _controller = new HealthController(_mockHealthCheckService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CheckHealth_WithValidUrl_ReturnsOkResult()
    {
        // Arrange
        var url = "https://example.com";
        var healthCheckResult = new HealthCheckResult
        {
            Url = url,
            IsHealthy = true,
            StatusCode = 200,
            ResponseTimeMs = 100,
            CheckedAt = DateTime.UtcNow,
            Error = null
        };

        _mockHealthCheckService
            .Setup(s => s.CheckHealthAsync(url, It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthCheckResult);

        // Act
        var result = await _controller.CheckHealth(url, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        var returnedResult = Assert.IsType<HealthCheckResult>(okResult.Value);
        Assert.Equal(url, returnedResult.Url);
        Assert.True(returnedResult.IsHealthy);
        Assert.Equal(200, returnedResult.StatusCode);
    }

    [Fact]
    public async Task CheckHealth_WithValidUrlButUnhealthyTarget_ReturnsOkResultWithIsHealthyFalse()
    {
        // Arrange
        var url = "https://example.com/notfound";
        var healthCheckResult = new HealthCheckResult
        {
            Url = url,
            IsHealthy = false,
            StatusCode = 404,
            ResponseTimeMs = 87,
            CheckedAt = DateTime.UtcNow,
            Error = "HTTP 404: Unhealthy response."
        };

        _mockHealthCheckService
            .Setup(s => s.CheckHealthAsync(url, It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthCheckResult);

        // Act
        var result = await _controller.CheckHealth(url, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResult = Assert.IsType<HealthCheckResult>(okResult.Value);
        Assert.False(returnedResult.IsHealthy);
        Assert.Equal(404, returnedResult.StatusCode);
        Assert.NotNull(returnedResult.Error);
    }

    [Fact]
    public async Task CheckHealth_WithNullUrl_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.CheckHealth(null, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task CheckHealth_WithEmptyUrl_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.CheckHealth("", CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task CheckHealth_WithWhitespaceOnlyUrl_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.CheckHealth("   ", CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task CheckHealth_WithValidationErrorFromService_ReturnsBadRequest()
    {
        // Arrange
        var url = "ftp://unsupported.com";
        var healthCheckResult = new HealthCheckResult
        {
            Url = url,
            IsHealthy = false,
            StatusCode = null,
            ResponseTimeMs = 0,
            CheckedAt = DateTime.UtcNow,
            Error = "Only HTTP and HTTPS schemes are allowed."
        };

        _mockHealthCheckService
            .Setup(s => s.CheckHealthAsync(url, It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthCheckResult);

        // Act
        var result = await _controller.CheckHealth(url, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task CheckHealth_WithInvalidUrl_ReturnsBadRequest()
    {
        // Arrange
        var url = "not-a-valid-url";
        var healthCheckResult = new HealthCheckResult
        {
            Url = url,
            IsHealthy = false,
            StatusCode = null,
            ResponseTimeMs = 0,
            CheckedAt = DateTime.UtcNow,
            Error = "URL is not a valid absolute URI."
        };

        _mockHealthCheckService
            .Setup(s => s.CheckHealthAsync(url, It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthCheckResult);

        // Act
        var result = await _controller.CheckHealth(url, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task CheckHealth_WithServerError_ReturnsOkWithErrorDetails()
    {
        // Arrange
        var url = "https://example.com/error";
        var healthCheckResult = new HealthCheckResult
        {
            Url = url,
            IsHealthy = false,
            StatusCode = 500,
            ResponseTimeMs = 152,
            CheckedAt = DateTime.UtcNow,
            Error = "HTTP 500: Unhealthy response."
        };

        _mockHealthCheckService
            .Setup(s => s.CheckHealthAsync(url, It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthCheckResult);

        // Act
        var result = await _controller.CheckHealth(url, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResult = Assert.IsType<HealthCheckResult>(okResult.Value);
        Assert.False(returnedResult.IsHealthy);
        Assert.Equal(500, returnedResult.StatusCode);
    }

    [Fact]
    public async Task CheckHealth_WithTimeout_ReturnsOkWithTimeoutError()
    {
        // Arrange
        var url = "https://slow.example.com";
        var healthCheckResult = new HealthCheckResult
        {
            Url = url,
            IsHealthy = false,
            StatusCode = null,
            ResponseTimeMs = 10000,
            CheckedAt = DateTime.UtcNow,
            Error = "Request timed out."
        };

        _mockHealthCheckService
            .Setup(s => s.CheckHealthAsync(url, It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthCheckResult);

        // Act
        var result = await _controller.CheckHealth(url, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResult = Assert.IsType<HealthCheckResult>(okResult.Value);
        Assert.False(returnedResult.IsHealthy);
        Assert.Null(returnedResult.StatusCode);
        Assert.Equal("Request timed out.", returnedResult.Error);
    }

    [Fact]
    public async Task CheckHealth_CallsServiceWithCorrectUrl()
    {
        // Arrange
        var url = "https://example.com";
        var healthCheckResult = new HealthCheckResult
        {
            Url = url,
            IsHealthy = true,
            StatusCode = 200,
            ResponseTimeMs = 100,
            CheckedAt = DateTime.UtcNow,
            Error = null
        };

        _mockHealthCheckService
            .Setup(s => s.CheckHealthAsync(url, It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthCheckResult);

        // Act
        await _controller.CheckHealth(url, CancellationToken.None);

        // Assert
        _mockHealthCheckService.Verify(
            s => s.CheckHealthAsync(url, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CheckHealth_PassesCancellationTokenToService()
    {
        // Arrange
        var url = "https://example.com";
        var healthCheckResult = new HealthCheckResult
        {
            Url = url,
            IsHealthy = true,
            StatusCode = 200,
            ResponseTimeMs = 100,
            CheckedAt = DateTime.UtcNow,
            Error = null
        };
        var cancellationToken = new CancellationToken();

        _mockHealthCheckService
            .Setup(s => s.CheckHealthAsync(url, cancellationToken))
            .ReturnsAsync(healthCheckResult);

        // Act
        await _controller.CheckHealth(url, cancellationToken);

        // Assert
        _mockHealthCheckService.Verify(
            s => s.CheckHealthAsync(url, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task CheckHealth_WithUrlContainingSpecialCharacters_ReturnsOkResult()
    {
        // Arrange
        var url = "https://example.com/api/v1/resource?id=123&name=test";
        var healthCheckResult = new HealthCheckResult
        {
            Url = url,
            IsHealthy = true,
            StatusCode = 200,
            ResponseTimeMs = 100,
            CheckedAt = DateTime.UtcNow,
            Error = null
        };

        _mockHealthCheckService
            .Setup(s => s.CheckHealthAsync(url, It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthCheckResult);

        // Act
        var result = await _controller.CheckHealth(url, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Theory]
    [InlineData(200, true)]
    [InlineData(201, true)]
    [InlineData(204, true)]
    [InlineData(299, true)]
    [InlineData(300, false)]
    [InlineData(400, false)]
    [InlineData(404, false)]
    [InlineData(500, false)]
    [InlineData(503, false)]
    public async Task CheckHealth_ReturnsCorrectHealthStatus(int statusCode, bool expectedIsHealthy)
    {
        // Arrange
        var url = "https://example.com";
        var healthCheckResult = new HealthCheckResult
        {
            Url = url,
            IsHealthy = expectedIsHealthy,
            StatusCode = statusCode,
            ResponseTimeMs = 100,
            CheckedAt = DateTime.UtcNow,
            Error = expectedIsHealthy ? null : $"HTTP {statusCode}: Unhealthy response."
        };

        _mockHealthCheckService
            .Setup(s => s.CheckHealthAsync(url, It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthCheckResult);

        // Act
        var result = await _controller.CheckHealth(url, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResult = Assert.IsType<HealthCheckResult>(okResult.Value);
        Assert.Equal(expectedIsHealthy, returnedResult.IsHealthy);
    }
}

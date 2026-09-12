using System.Net;
using ApiHealthMonitor.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ApiHealthMonitor.Tests.Integration;

public class HealthCheckIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public HealthCheckIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CheckHealth_Endpoint_WithValidUrl_Returns200Ok()
    {
        // Arrange
        var url = "https://www.google.com";
        var queryParam = Uri.EscapeDataString(url);

        // Act
        var response = await _client.GetAsync($"/api/health/check?url={queryParam}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task CheckHealth_Endpoint_WithEmptyUrl_Returns400BadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/health/check?url=");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("error", content.ToLowerInvariant());
    }

    [Fact]
    public async Task CheckHealth_Endpoint_WithoutUrlParameter_Returns400BadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/health/check");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CheckHealth_Endpoint_WithInvalidUrl_Returns400BadRequest()
    {
        // Arrange
        var url = "invalid-url";

        // Act
        var response = await _client.GetAsync($"/api/health/check?url={url}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CheckHealth_Endpoint_WithUnsupportedScheme_Returns400BadRequest()
    {
        // Arrange
        var url = Uri.EscapeDataString("ftp://files.example.com");

        // Act
        var response = await _client.GetAsync($"/api/health/check?url={url}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CheckHealth_Endpoint_ReturnsCorrectJsonShape()
    {
        // Arrange
        var url = "https://www.google.com";
        var queryParam = Uri.EscapeDataString(url);

        // Act
        var response = await _client.GetAsync($"/api/health/check?url={queryParam}");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("\"url\"", content);
        Assert.Contains("\"isHealthy\"", content);
        Assert.Contains("\"statusCode\"", content);
        Assert.Contains("\"responseTimeMs\"", content);
        Assert.Contains("\"checkedAt\"", content);
        Assert.Contains("\"error\"", content);
    }

    [Fact]
    public async Task CheckHealth_Endpoint_ReturnsHttpResponseWithCorrectContentType()
    {
        // Arrange
        var url = "https://www.google.com";
        var queryParam = Uri.EscapeDataString(url);

        // Act
        var response = await _client.GetAsync($"/api/health/check?url={queryParam}");

        // Assert
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Contains("application/json", response.Content.Headers.ContentType.ToString());
    }

    [Fact]
    public async Task CheckHealth_Endpoint_WithUrlContainingQueryString_Returns200Ok()
    {
        // Arrange
        var url = "https://www.google.com/search?q=test";
        var queryParam = Uri.EscapeDataString(url);

        // Act
        var response = await _client.GetAsync($"/api/health/check?url={queryParam}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CheckHealth_Endpoint_WithHttpsUrl_Returns200Ok()
    {
        // Arrange
        var url = "https://www.example.com";
        var queryParam = Uri.EscapeDataString(url);

        // Act
        var response = await _client.GetAsync($"/api/health/check?url={queryParam}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CheckHealth_Endpoint_WithHttpUrl_Returns200Ok()
    {
        // Arrange
        var url = "http://www.example.com";
        var queryParam = Uri.EscapeDataString(url);

        // Act
        var response = await _client.GetAsync($"/api/health/check?url={queryParam}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CheckHealth_Endpoint_BadRequestResponse_IncludesErrorField()
    {
        // Act
        var response = await _client.GetAsync("/api/health/check?url=invalid");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"error\"", content);
    }

    [Fact]
    public async Task CheckHealth_Endpoint_ValidUrlResponse_IncludesUrlField()
    {
        // Arrange
        var url = "https://www.google.com";
        var queryParam = Uri.EscapeDataString(url);

        // Act
        var response = await _client.GetAsync($"/api/health/check?url={queryParam}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains(Uri.EscapeDataString(url), content);
    }

    [Fact]
    public async Task CheckHealth_Endpoint_WithMalformedUrl_Returns400BadRequest()
    {
        // Arrange
        var url = "ht!tp://invalid[url].com";

        // Act
        var response = await _client.GetAsync($"/api/health/check?url={url}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CheckHealth_Endpoint_WithUrlContainingPort_Returns200Ok()
    {
        // Arrange
        var url = "https://www.google.com:443/";
        var queryParam = Uri.EscapeDataString(url);

        // Act
        var response = await _client.GetAsync($"/api/health/check?url={queryParam}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

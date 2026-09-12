using ApiHealthMonitor.Services;
using ApiHealthMonitor.Validation;
using Xunit;

namespace ApiHealthMonitor.Tests.Services;

public class UrlValidatorTests
{
    [Fact]
    public void ValidateUrl_WithValidHttpUrl_ReturnsNull()
    {
        // Arrange
        var url = "http://example.com";

        // Act
        var result = UrlValidator.ValidateUrl(url);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ValidateUrl_WithValidHttpsUrl_ReturnsNull()
    {
        // Arrange
        var url = "https://example.com/api/endpoint";

        // Act
        var result = UrlValidator.ValidateUrl(url);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ValidateUrl_WithUrlContainingQueryString_ReturnsNull()
    {
        // Arrange
        var url = "https://example.com/api?key=value&other=123";

        // Act
        var result = UrlValidator.ValidateUrl(url);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ValidateUrl_WithNullUrl_ReturnsError()
    {
        // Arrange
        string? url = null;

        // Act
        var result = UrlValidator.ValidateUrl(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ValidationConstants.ErrorUrlEmpty, result);
    }

    [Fact]
    public void ValidateUrl_WithEmptyUrl_ReturnsError()
    {
        // Arrange
        var url = "";

        // Act
        var result = UrlValidator.ValidateUrl(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ValidationConstants.ErrorUrlEmpty, result);
    }

    [Fact]
    public void ValidateUrl_WithWhitespaceOnlyUrl_ReturnsError()
    {
        // Arrange
        var url = "   ";

        // Act
        var result = UrlValidator.ValidateUrl(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ValidationConstants.ErrorUrlEmpty, result);
    }

    [Fact]
    public void ValidateUrl_WithUrlExceedingMaxLength_ReturnsError()
    {
        // Arrange
        var url = "https://example.com/" + new string('a', ValidationConstants.MaxUrlLength);

        // Act
        var result = UrlValidator.ValidateUrl(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ValidationConstants.ErrorUrlTooLong, result);
    }

    [Fact]
    public void ValidateUrl_WithMalformedUrl_ReturnsError()
    {
        // Arrange
        var url = "not a url at all";

        // Act
        var result = UrlValidator.ValidateUrl(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ValidationConstants.ErrorUrlInvalid, result);
    }

    [Fact]
    public void ValidateUrl_WithRelativeUrl_ReturnsError()
    {
        // Arrange
        var url = "/api/endpoint";

        // Act
        var result = UrlValidator.ValidateUrl(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ValidationConstants.ErrorUrlInvalid, result);
    }

    [Fact]
    public void ValidateUrl_WithFtpScheme_ReturnsError()
    {
        // Arrange
        var url = "ftp://files.example.com";

        // Act
        var result = UrlValidator.ValidateUrl(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ValidationConstants.ErrorSchemeNotAllowed, result);
    }

    [Fact]
    public void ValidateUrl_WithFileScheme_ReturnsError()
    {
        // Arrange
        var url = "file:///etc/passwd";

        // Act
        var result = UrlValidator.ValidateUrl(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ValidationConstants.ErrorSchemeNotAllowed, result);
    }

    [Fact]
    public void ValidateUrl_WithDataScheme_ReturnsError()
    {
        // Arrange
        var url = "data:text/html,<h1>test</h1>";

        // Act
        var result = UrlValidator.ValidateUrl(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ValidationConstants.ErrorSchemeNotAllowed, result);
    }

    [Theory]
    [InlineData("http://127.0.0.1")]
    [InlineData("http://127.0.0.1:8080")]
    [InlineData("http://localhost")]
    [InlineData("http://localhost:5000")]
    public void ValidateUrl_WithLocalhostAndPrivateNetworksEnabled_ReturnsError(string url)
    {
        // Act
        var result = UrlValidator.ValidateUrl(url, blockPrivateNetworks: true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ValidationConstants.ErrorPrivateNetworkBlocked, result);
    }

    [Theory]
    [InlineData("http://10.0.0.1")]
    [InlineData("http://10.255.255.255")]
    [InlineData("http://172.16.0.1")]
    [InlineData("http://172.31.255.255")]
    [InlineData("http://192.168.1.1")]
    [InlineData("http://192.168.255.255")]
    public void ValidateUrl_WithPrivateIpv4RangesAndPrivateNetworksEnabled_ReturnsError(string url)
    {
        // Act
        var result = UrlValidator.ValidateUrl(url, blockPrivateNetworks: true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ValidationConstants.ErrorPrivateNetworkBlocked, result);
    }

    [Theory]
    [InlineData("http://169.254.1.1")]
    public void ValidateUrl_WithLinkLocalAddressAndPrivateNetworksEnabled_ReturnsError(string url)
    {
        // Act
        var result = UrlValidator.ValidateUrl(url, blockPrivateNetworks: true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ValidationConstants.ErrorPrivateNetworkBlocked, result);
    }

    [Theory]
    [InlineData("http://127.0.0.1")]
    [InlineData("http://localhost")]
    [InlineData("http://10.0.0.1")]
    [InlineData("http://192.168.1.1")]
    public void ValidateUrl_WithPrivateNetworkAndPrivateNetworksDisabled_ReturnsNull(string url)
    {
        // Act
        var result = UrlValidator.ValidateUrl(url, blockPrivateNetworks: false);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("http://8.8.8.8")]
    [InlineData("https://1.1.1.1")]
    [InlineData("http://example.com")]
    [InlineData("https://google.com")]
    public void ValidateUrl_WithPublicIpAndPrivateNetworksEnabled_ReturnsNull(string url)
    {
        // Act
        var result = UrlValidator.ValidateUrl(url, blockPrivateNetworks: true);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ValidateUrl_WithIPv6Localhost_BlocksWhenPrivateNetworksEnabled()
    {
        // Arrange
        var url = "http://[::1]";

        // Act
        var result = UrlValidator.ValidateUrl(url, blockPrivateNetworks: true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ValidationConstants.ErrorPrivateNetworkBlocked, result);
    }

    [Fact]
    public void ValidateUrl_WithUrlContainingPort_ReturnsNull()
    {
        // Arrange
        var url = "https://example.com:8443/api/v1/status";

        // Act
        var result = UrlValidator.ValidateUrl(url);

        // Assert
        Assert.Null(result);
    }
}

using ApiHealthMonitor.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace ApiHealthMonitor.Tests.Configuration;

public class HealthCheckOptionsTests
{
    [Fact]
    public void HealthCheckOptions_HasCorrectSectionName()
    {
        // Assert
        Assert.Equal("HealthCheck", HealthCheckOptions.SectionName);
    }

    [Fact]
    public void HealthCheckOptions_HasDefaultValues()
    {
        // Arrange & Act
        var options = new HealthCheckOptions();

        // Assert
        Assert.Equal(10, options.TimeoutSeconds);
        Assert.Equal("ApiHealthMonitor/1.0", options.UserAgent);
        Assert.False(options.BlockPrivateNetworks);
        Assert.Equal(10 * 1024 * 1024, options.MaxResponseSizeBytes);
    }

    [Fact]
    public void HealthCheckOptions_CanBeConfigured()
    {
        // Arrange & Act
        var options = new HealthCheckOptions
        {
            TimeoutSeconds = 30,
            UserAgent = "CustomMonitor/2.0",
            BlockPrivateNetworks = true,
            MaxResponseSizeBytes = 5 * 1024 * 1024
        };

        // Assert
        Assert.Equal(30, options.TimeoutSeconds);
        Assert.Equal("CustomMonitor/2.0", options.UserAgent);
        Assert.True(options.BlockPrivateNetworks);
        Assert.Equal(5 * 1024 * 1024, options.MaxResponseSizeBytes);
    }

    [Fact]
    public void HealthCheckOptions_CanBeLoadedFromConfiguration()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "HealthCheck:TimeoutSeconds", "15" },
                { "HealthCheck:UserAgent", "TestMonitor/1.0" },
                { "HealthCheck:BlockPrivateNetworks", "true" },
                { "HealthCheck:MaxResponseSizeBytes", "1048576" }
            });

        var config = configBuilder.Build();

        // Act
        var options = new HealthCheckOptions();
        config.GetSection(HealthCheckOptions.SectionName).Bind(options);

        // Assert
        Assert.Equal(15, options.TimeoutSeconds);
        Assert.Equal("TestMonitor/1.0", options.UserAgent);
        Assert.True(options.BlockPrivateNetworks);
        Assert.Equal(1048576, options.MaxResponseSizeBytes);
    }

    [Fact]
    public void HealthCheckOptions_WithDependencyInjection_ConfiguresCorrectly()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "HealthCheck:TimeoutSeconds", "20" },
                { "HealthCheck:UserAgent", "DIMonitor/1.0" }
            });

        var config = configBuilder.Build();
        var services = new ServiceCollection();

        // Act
        services.Configure<HealthCheckOptions>(
            config.GetSection(HealthCheckOptions.SectionName));

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckOptions>>();

        // Assert
        Assert.Equal(20, options.Value.TimeoutSeconds);
        Assert.Equal("DIMonitor/1.0", options.Value.UserAgent);
    }

    [Fact]
    public void HealthCheckOptions_MissingConfigValues_UsesDefaults()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "HealthCheck:TimeoutSeconds", "25" }
                // Other values are not provided
            });

        var config = configBuilder.Build();

        // Act
        var options = new HealthCheckOptions();
        config.GetSection(HealthCheckOptions.SectionName).Bind(options);

        // Assert
        Assert.Equal(25, options.TimeoutSeconds);
        Assert.Equal("ApiHealthMonitor/1.0", options.UserAgent); // Default
        Assert.False(options.BlockPrivateNetworks); // Default
        Assert.Equal(10 * 1024 * 1024, options.MaxResponseSizeBytes); // Default
    }

    [Fact]
    public void HealthCheckOptions_PartialConfiguration_KeepsDefaults()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "HealthCheck:BlockPrivateNetworks", "true" }
            });

        var config = configBuilder.Build();

        // Act
        var options = new HealthCheckOptions();
        config.GetSection(HealthCheckOptions.SectionName).Bind(options);

        // Assert
        Assert.Equal(10, options.TimeoutSeconds); // Default
        Assert.Equal("ApiHealthMonitor/1.0", options.UserAgent); // Default
        Assert.True(options.BlockPrivateNetworks); // Configured
        Assert.Equal(10 * 1024 * 1024, options.MaxResponseSizeBytes); // Default
    }

    [Fact]
    public void HealthCheckOptions_InvalidConfigValues_UsesDefaults()
    {
        // Arrange
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "HealthCheck:TimeoutSeconds", "not-a-number" }
            });

        var config = configBuilder.Build();

        // Act
        var options = new HealthCheckOptions();
        config.GetSection(HealthCheckOptions.SectionName).Bind(options);

        // Assert - Invalid values might not bind, keeping defaults
        Assert.Equal(10, options.TimeoutSeconds); // Default, as binding failed
    }
}

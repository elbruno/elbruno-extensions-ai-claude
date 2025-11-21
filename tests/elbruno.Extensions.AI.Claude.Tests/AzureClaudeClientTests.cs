using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.AI;

namespace elbruno.Extensions.AI.Claude.Tests;

public class AzureClaudeClientTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var endpoint = new Uri("https://example.services.ai.azure.com");
        var deploymentName = "claude-sonnet-4-5";
        var credential = new DefaultAzureCredential();

        // Act
        var client = new AzureClaudeClient(endpoint, deploymentName, credential);

        // Assert
        Assert.NotNull(client);
        Assert.Equal("Microsoft Foundry", client.Metadata.ProviderName);
        Assert.Equal(deploymentName, client.Metadata.ModelId);
    }

    [Fact]
    public void Constructor_WithNullEndpoint_ShouldThrowArgumentNullException()
    {
        // Arrange
        Uri? endpoint = null;
        var deploymentName = "claude-sonnet-4-5";
        var credential = new DefaultAzureCredential();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new AzureClaudeClient(endpoint!, deploymentName, credential));
    }

    [Fact]
    public void Constructor_WithNullModelId_ShouldThrowArgumentNullException()
    {
        // Arrange
        var endpoint = new Uri("https://example.services.ai.azure.com");
        string? deploymentName = null;
        var credential = new DefaultAzureCredential();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new AzureClaudeClient(endpoint, deploymentName!, credential));
    }

    [Fact]
    public void Constructor_WithNullCredential_ShouldThrowArgumentNullException()
    {
        // Arrange
        var endpoint = new Uri("https://example.services.ai.azure.com");
        var deploymentName = "claude-sonnet-4-5";
        TokenCredential? credential = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new AzureClaudeClient(endpoint, deploymentName, credential!));
    }

    [Fact]
    public void GetService_WithMatchingType_ShouldReturnClient()
    {
        // Arrange
        var endpoint = new Uri("https://example.services.ai.azure.com");
        var deploymentName = "claude-sonnet-4-5";
        var credential = new DefaultAzureCredential();
        var client = new AzureClaudeClient(endpoint, deploymentName, credential);

        // Act
        var service = client.GetService(typeof(IChatClient));

        // Assert
        Assert.NotNull(service);
        Assert.Same(client, service);
    }

    [Fact]
    public void GetService_WithNonMatchingType_ShouldReturnNull()
    {
        // Arrange
        var endpoint = new Uri("https://example.services.ai.azure.com");
        var deploymentName = "claude-sonnet-4-5";
        var credential = new DefaultAzureCredential();
        var client = new AzureClaudeClient(endpoint, deploymentName, credential);

        // Act
        var service = client.GetService(typeof(string));

        // Assert
        Assert.Null(service);
    }
}

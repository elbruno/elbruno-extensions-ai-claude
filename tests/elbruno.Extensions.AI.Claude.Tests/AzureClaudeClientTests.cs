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
        var endpoint = new Uri("https://example.cognitiveservices.azure.com");
        var modelId = "claude-3-5-sonnet-20241022";
        var credential = new DefaultAzureCredential();

        // Act
        var client = new AzureClaudeClient(endpoint, modelId, credential);

        // Assert
        Assert.NotNull(client);
        Assert.Equal("Azure AI Foundry", client.Metadata.ProviderName);
        Assert.Equal(modelId, client.Metadata.ModelId);
    }

    [Fact]
    public void Constructor_WithNullEndpoint_ShouldThrowArgumentNullException()
    {
        // Arrange
        Uri? endpoint = null;
        var modelId = "claude-3-5-sonnet-20241022";
        var credential = new DefaultAzureCredential();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new AzureClaudeClient(endpoint!, modelId, credential));
    }

    [Fact]
    public void Constructor_WithNullModelId_ShouldThrowArgumentNullException()
    {
        // Arrange
        var endpoint = new Uri("https://example.cognitiveservices.azure.com");
        string? modelId = null;
        var credential = new DefaultAzureCredential();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new AzureClaudeClient(endpoint, modelId!, credential));
    }

    [Fact]
    public void Constructor_WithNullCredential_ShouldThrowArgumentNullException()
    {
        // Arrange
        var endpoint = new Uri("https://example.cognitiveservices.azure.com");
        var modelId = "claude-3-5-sonnet-20241022";
        TokenCredential? credential = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new AzureClaudeClient(endpoint, modelId, credential!));
    }

    [Fact]
    public void GetService_WithMatchingType_ShouldReturnClient()
    {
        // Arrange
        var endpoint = new Uri("https://example.cognitiveservices.azure.com");
        var modelId = "claude-3-5-sonnet-20241022";
        var credential = new DefaultAzureCredential();
        var client = new AzureClaudeClient(endpoint, modelId, credential);

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
        var endpoint = new Uri("https://example.cognitiveservices.azure.com");
        var modelId = "claude-3-5-sonnet-20241022";
        var credential = new DefaultAzureCredential();
        var client = new AzureClaudeClient(endpoint, modelId, credential);

        // Act
        var service = client.GetService(typeof(string));

        // Assert
        Assert.Null(service);
    }
}

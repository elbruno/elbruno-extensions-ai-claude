using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.AI;
using System.Net;
using System.Text;

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

    [Fact]
    public void Constructor_WithApiKey_ShouldSucceed()
    {
        // Arrange
        var endpoint = new Uri("https://example.services.ai.azure.com");
        var deploymentName = "claude-sonnet-4-5";

        // Act
        var client = new AzureClaudeClient(endpoint, deploymentName, "test-api-key");

        // Assert
        Assert.NotNull(client);
        Assert.Equal(deploymentName, client.Metadata.ModelId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithInvalidApiKey_ShouldThrowArgumentException(string apiKey)
    {
        // Arrange
        var endpoint = new Uri("https://example.services.ai.azure.com");
        var deploymentName = "claude-sonnet-4-5";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new AzureClaudeClient(endpoint, deploymentName, apiKey));
    }

    [Fact]
    public async Task CompleteAsync_WithApiKey_ShouldSendHeader()
    {
        // Arrange
        var endpoint = new Uri("https://example.services.ai.azure.com");
        var deploymentName = "claude-sonnet-4-5";

        const string jsonResponse = """
        {
          "id": "abc123",
          "model": "claude-sonnet-4-5",
          "stop_reason": "end_turn",
          "content": [{"type": "text", "text": "hello there"}],
          "usage": {"input_tokens": 10, "output_tokens": 5}
        }
        """;

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        var handler = new TestHandler(response);
        var httpClient = new HttpClient(handler);
        var client = new AzureClaudeClient(endpoint, deploymentName, "test-api-key", httpClient);

        // Act
        var result = await client.CompleteAsync([new ChatMessage(ChatRole.User, "ping")]);

        // Assert
        Assert.NotNull(handler.LastRequest);
        Assert.True(handler.LastRequest!.Headers.TryGetValues("x-api-key", out var values));
        Assert.Equal("test-api-key", values!.Single());
        Assert.Equal("hello there", result.Message.Text);
    }

    private sealed class TestHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public TestHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(_response);
        }
    }
}

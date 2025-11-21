# elbruno.Extensions.AI.Claude

A C# library for using Claude models deployed in Azure AI Foundry, implementing the `Microsoft.Extensions.AI` abstractions.

## Overview

This library provides an `AzureClaudeClient` that allows you to interact with Claude models (e.g., Claude 3.5 Sonnet) deployed in Azure AI Foundry. It follows the same patterns as `AzureOpenAIClient` and implements the standard `IChatClient` interface from `Microsoft.Extensions.AI`.

## Features

- ✅ Full `IChatClient` implementation
- ✅ Support for both streaming and non-streaming completions
- ✅ Azure authentication using Azure Identity
- ✅ System messages support
- ✅ Multi-turn conversations
- ✅ Token usage tracking
- ✅ Configurable chat options (temperature, top-p, max tokens)

## Installation

```bash
dotnet add package elbruno.Extensions.AI.Claude
```

## Prerequisites

1. An Azure subscription
2. A Claude model deployed in Azure AI Foundry
3. Appropriate Azure credentials configured

## Quick Start

```csharp
using Azure.Identity;
using elbruno.Extensions.AI.Claude;
using Microsoft.Extensions.AI;

// Configure the client
var endpoint = new Uri("https://your-endpoint.cognitiveservices.azure.com");
var modelId = "claude-3-5-sonnet-20241022";
var credential = new DefaultAzureCredential();

// Create the client
var client = new AzureClaudeClient(endpoint, modelId, credential);

// Send a message
var messages = new List<ChatMessage>
{
    new(ChatRole.User, "What is the capital of France?")
};

var response = await client.CompleteAsync(messages);
Console.WriteLine(response.Message.Text);
```

## Usage Examples

### Simple Completion

```csharp
var messages = new List<ChatMessage>
{
    new(ChatRole.User, "Explain quantum computing in simple terms.")
};

var response = await client.CompleteAsync(messages);
Console.WriteLine(response.Message.Text);
Console.WriteLine($"Tokens used: {response.Usage?.TotalTokenCount}");
```

### Streaming Completion

```csharp
var messages = new List<ChatMessage>
{
    new(ChatRole.User, "Write a short story about a robot.")
};

await foreach (var update in client.CompleteStreamingAsync(messages))
{
    if (!string.IsNullOrEmpty(update.Text))
    {
        Console.Write(update.Text);
    }
}
```

### Multi-turn Conversation

```csharp
var conversation = new List<ChatMessage>
{
    new(ChatRole.System, "You are a helpful coding assistant."),
    new(ChatRole.User, "How do I create a list in Python?"),
};

var response1 = await client.CompleteAsync(conversation);
conversation.Add(response1.Message);

conversation.Add(new ChatMessage(ChatRole.User, "Can you show me an example?"));
var response2 = await client.CompleteAsync(conversation);
```

### With Chat Options

```csharp
var options = new ChatOptions
{
    Temperature = 0.7,
    TopP = 0.9,
    MaxOutputTokens = 2048
};

var messages = new List<ChatMessage>
{
    new(ChatRole.User, "Generate creative ideas for a tech startup.")
};

var response = await client.CompleteAsync(messages, options);
```

## Configuration

### Environment Variables

You can configure the client using environment variables:

```bash
export AZURE_CLAUDE_ENDPOINT="https://your-endpoint.cognitiveservices.azure.com"
export AZURE_CLAUDE_MODEL="claude-3-5-sonnet-20241022"
```

### Authentication

The library uses Azure Identity for authentication. You can use any of the following:

- `DefaultAzureCredential` (recommended for development and production)
- `ManagedIdentityCredential` (for Azure resources)
- `EnvironmentCredential` (for environment variables)
- `AzureCliCredential` (for local development)

Example with specific credential:

```csharp
var credential = new ManagedIdentityCredential();
var client = new AzureClaudeClient(endpoint, modelId, credential);
```

## Supported Models

This library supports Claude models deployed in Azure AI Foundry, including:

- Claude 3.5 Sonnet (claude-3-5-sonnet-20241022)
- Claude 3 Opus
- Claude 3 Sonnet
- Other Claude variants available in Azure AI Foundry

## API Reference

### AzureClaudeClient

#### Constructor

```csharp
public AzureClaudeClient(
    Uri endpoint,
    string modelId,
    TokenCredential credential,
    HttpClient? httpClient = null)
```

#### Properties

- `Metadata` - Returns `ChatClientMetadata` with provider name and model ID

#### Methods

- `CompleteAsync` - Sends a completion request and returns the full response
- `CompleteStreamingAsync` - Sends a streaming completion request
- `GetService` - Gets a service of the specified type
- `Dispose` - Disposes resources

## Building the Project

```bash
# Clone the repository
git clone https://github.com/elbruno/elbruno-extensions-ai-claude.git
cd elbruno-extensions-ai-claude

# Build the solution
dotnet build

# Run tests
dotnet test

# Pack the NuGet package
dotnet pack src/elbruno.Extensions.AI.Claude/elbruno.Extensions.AI.Claude.csproj
```

## Testing

The project includes unit tests for core functionality. Run them with:

```bash
dotnet test
```

## Sample Application

A sample console application is included in the `samples` directory. To run it:

```bash
# Set environment variables
export AZURE_CLAUDE_ENDPOINT="your-endpoint"
export AZURE_CLAUDE_MODEL="your-model-id"

# Run the sample
dotnet run --project samples/elbruno.Extensions.AI.Claude.Samples
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## References

- [Azure AI Foundry Documentation](https://learn.microsoft.com/azure/ai-studio/)
- [Microsoft.Extensions.AI Documentation](https://learn.microsoft.com/dotnet/api/microsoft.extensions.ai)
- [Claude API Documentation](https://docs.anthropic.com/claude/reference)
- [Azure Identity Library](https://learn.microsoft.com/dotnet/api/azure.identity)

## Support

For issues and questions:
- Open an issue on [GitHub](https://github.com/elbruno/elbruno-extensions-ai-claude/issues)
- Contact: El Bruno

## Changelog

### 0.1.0 (Initial Release)
- Initial implementation of AzureClaudeClient
- Support for completions and streaming
- Azure authentication support
- Full IChatClient interface implementation

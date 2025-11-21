# elbruno.Extensions.AI.Claude

A C# library for using Claude models deployed in Microsoft Foundry, implementing the `Microsoft.Extensions.AI` abstractions.

## Overview

This library provides an `AzureClaudeClient` that allows you to interact with Claude models (e.g., Claude Sonnet 4.5) deployed in Microsoft Foundry. It follows the same patterns as `AzureOpenAIClient` and implements the standard `IChatClient` interface from `Microsoft.Extensions.AI`.

## Features

- ✅ Full `IChatClient` implementation
- ✅ Support for both streaming and non-streaming completions
- ✅ Azure authentication using Azure Identity or API keys
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
2. A Claude model deployed in Microsoft Foundry
3. Appropriate Azure credentials configured

## Quick Start

### Authenticate with DefaultAzureCredential (recommended)

```csharp
using Azure.Identity;
using elbruno.Extensions.AI.Claude;
using Microsoft.Extensions.AI;

var endpoint = new Uri("https://your-endpoint.services.ai.azure.com");
var deploymentName = "claude-sonnet-4-5";
var credential = new DefaultAzureCredential();

var client = new AzureClaudeClient(endpoint, deploymentName, credential);

var messages = new List<ChatMessage>
{
    new(ChatRole.User, "What is the capital of France?")
};

var response = await client.CompleteAsync(messages);
Console.WriteLine(response.Message.Text);
```

### Authenticate with an API key (when managed identity isn't available)

```csharp
using elbruno.Extensions.AI.Claude;
using Microsoft.Extensions.AI;

var endpoint = new Uri("https://your-endpoint.services.ai.azure.com");
var deploymentName = "claude-sonnet-4-5";
var apiKey = Environment.GetEnvironmentVariable("AZURE_CLAUDE_APIKEY")
    ?? throw new InvalidOperationException("Store the API key securely (for example in Azure Key Vault) and expose it via configuration.");

var client = new AzureClaudeClient(endpoint, deploymentName, apiKey);

var response = await client.CompleteAsync(new List<ChatMessage>
{
    new(ChatRole.User, "Share two secure ways to store secrets in Azure.")
});

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
export AZURE_CLAUDE_ENDPOINT="https://your-endpoint.services.ai.azure.com"
export AZURE_CLAUDE_MODEL="claude-sonnet-4-5"
export AZURE_CLAUDE_APIKEY="your-api-key" # Prefer Azure Key Vault or managed identity instead of plain env vars
```

### Authentication

The library supports two authentication flows:

1. **Azure Identity (recommended)** – Works with `DefaultAzureCredential`, `ManagedIdentityCredential`, `EnvironmentCredential`, `AzureCliCredential`, etc. Use this whenever you can rely on managed identity, service principals, or developer credentials.
2. **API Key authentication** – Useful for constrained environments where Azure Identity is not available. Store API keys in Azure Key Vault or another secure secret store; never hardcode them.

### Using Azure Identity

- `DefaultAzureCredential` (recommended for development and production)
- `ManagedIdentityCredential` (for Azure resources)
- `EnvironmentCredential` (for environment variables)
- `AzureCliCredential` (for local development)

Example with specific credential:

```csharp
var credential = new ManagedIdentityCredential();
var client = new AzureClaudeClient(endpoint, deploymentName, credential);
```

> **Note on Azure AI Foundry headers**
>
> Claude deployments hosted in Azure AI Foundry require the `anthropic-version: 2023-06-01` header for every request. Version `0.1.0-preview.3` of this library automatically applies it for both Managed Identity and API key scenarios. If you're pinned to an older version, add the header manually or upgrade to avoid `invalid_request_error` responses.

### Using an API key

```csharp
var apiKey = configuration["AZURE_CLAUDE_APIKEY"]
    ?? throw new InvalidOperationException("Configure AZURE_CLAUDE_APIKEY via Key Vault, Azure App Configuration, or user secrets.");

var client = new AzureClaudeClient(endpoint, deploymentName, apiKey);
```

## Supported Models

This library supports Claude models deployed in Microsoft Foundry through global standard deployment. The following models are currently available:

### Available Models

- **Claude Sonnet 4.5** (deployment name: `claude-sonnet-4-5`) - Anthropic's most capable model for building real-world agents and handling complex, long-horizon tasks. Best for agentic workflows, computer use capabilities, and production deployments.

- **Claude Haiku 4.5** (deployment name: `claude-haiku-4-5`) - Delivers near-frontier performance with optimal speed and cost. One of the best coding and agent models, ideal for free products and scaled sub-agents.

- **Claude Opus 4.1** (deployment name: `claude-opus-4-1`) - Industry leader for coding. Delivers sustained performance on long-running tasks requiring focused effort and thousands of steps, significantly expanding what AI agents can solve.

All models support:

- 1 million token context window
- Extended thinking for enhanced reasoning
- Image and text input
- Code generation, analysis, and debugging (Sonnet 4.5 and Opus 4.1)

### Choosing the Right Model

- **Claude Sonnet 4.5**: Best for balanced performance and capabilities, production workflows, complex reasoning, and agentic tasks
- **Claude Haiku 4.5**: Best for speed and cost optimization, high-volume processing, fast responses
- **Claude Opus 4.1**: Best for complex coding tasks, long-running agent workflows, and enterprise applications requiring sustained performance

## API Reference

### AzureClaudeClient

#### Constructor

```csharp
public AzureClaudeClient(
    Uri endpoint,
    string modelId,
    TokenCredential credential,
    HttpClient? httpClient = null)

public AzureClaudeClient(
    Uri endpoint,
    string modelId,
    string apiKey,
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

## Sample Applications

Two console applications ship in the `samples` folder:

1. `elbruno.Extensions.AI.Claude.Samples` – demonstrates Azure Default Credentials (managed identity flow).
2. `elbruno.Extensions.AI.Claude.ApiKeySample` – demonstrates API key authentication. Store `AZURE_CLAUDE_APIKEY` securely and load it through configuration/User Secrets.

Run the Default Credential sample:

```bash
# Set environment variables
export AZURE_CLAUDE_ENDPOINT="your-endpoint"
export AZURE_CLAUDE_MODEL="claude-sonnet-4-5"

# Run the sample
dotnet run --project samples/elbruno.Extensions.AI.Claude.Samples
```

Run the API key sample (after setting `AZURE_CLAUDE_APIKEY` securely):

```bash
export AZURE_CLAUDE_ENDPOINT="your-endpoint"
export AZURE_CLAUDE_MODEL="claude-sonnet-4-5"
export AZURE_CLAUDE_APIKEY="your-api-key"

dotnet run --project samples/elbruno.Extensions.AI.Claude.ApiKeySample
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## References

- [Microsoft Foundry Documentation](https://learn.microsoft.com/azure/ai-foundry/)
- [Deploy and use Claude models in Microsoft Foundry](https://learn.microsoft.com/azure/ai-foundry/foundry-models/how-to/use-foundry-models-claude)
- [Microsoft.Extensions.AI Documentation](https://learn.microsoft.com/dotnet/api/microsoft.extensions.ai)
- [Claude API Documentation](https://docs.anthropic.com/claude/reference)
- [Azure Identity Library](https://learn.microsoft.com/dotnet/api/azure.identity)
- [Generative AI for Beginners - .NET](https://aka.ms/genainet)

## Support

For issues and questions:

- Open an issue on [GitHub](https://github.com/elbruno/elbruno-extensions-ai-claude/issues)
- Contact: El Bruno

## Changelog

### 0.1.0-preview.2

- Added API key authentication constructor and helper methods
- Added dedicated API key console sample and documentation
- Updated NuGet metadata and release notes

### 0.1.0-preview.3

- Fixed `DefaultAzureCredential` flow by always including the required `anthropic-version` header
- Documented the Claude header requirement in Azure AI Foundry

### 0.1.0 (Initial Release)

- Initial implementation of AzureClaudeClient
- Support for completions and streaming
- Azure authentication support
- Full IChatClient interface implementation

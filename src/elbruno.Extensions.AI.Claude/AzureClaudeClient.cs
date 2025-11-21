using Azure.Core;
using Microsoft.Extensions.AI;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace elbruno.Extensions.AI.Claude;

/// <summary>
/// Client for interacting with Claude models deployed in Azure AI Foundry.
/// </summary>
public sealed class AzureClaudeClient : IChatClient
{
    private readonly Uri _endpoint;
    private readonly string _modelId;
    private readonly TokenCredential _credential;
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureClaudeClient"/> class.
    /// </summary>
    /// <param name="endpoint">The Azure AI Foundry endpoint URL.</param>
    /// <param name="modelId">The model ID (e.g., "claude-3-5-sonnet-20241022").</param>
    /// <param name="credential">The Azure credential for authentication.</param>
    /// <param name="httpClient">Optional HttpClient instance.</param>
    public AzureClaudeClient(
        Uri endpoint,
        string modelId,
        TokenCredential credential,
        HttpClient? httpClient = null)
    {
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        _modelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _credential = credential ?? throw new ArgumentNullException(nameof(credential));
        _httpClient = httpClient ?? new HttpClient();
    }

    /// <summary>
    /// Gets metadata about the chat client.
    /// </summary>
    public ChatClientMetadata Metadata => new(providerName: "Azure AI Foundry", modelId: _modelId);

    /// <summary>
    /// Sends a chat completion request.
    /// </summary>
    public async Task<ChatCompletion> CompleteAsync(
        IList<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var request = BuildRequest(chatMessages, options, stream: false);
        var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
        return ParseResponse(response);
    }

    /// <summary>
    /// Sends a streaming chat completion request.
    /// </summary>
    public async IAsyncEnumerable<StreamingChatCompletionUpdate> CompleteStreamingAsync(
        IList<ChatMessage> chatMessages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = BuildRequest(chatMessages, options, stream: true);
        
        await using var stream = await SendStreamingRequestAsync(request, cancellationToken).ConfigureAwait(false);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (!line.StartsWith("data: ")) continue;
            
            var data = line.Substring(6);
            if (data == "[DONE]") break;

            var update = ParseStreamingUpdate(data);
            if (update != null)
            {
                yield return update;
            }
        }
    }

    /// <summary>
    /// Gets the service for a specified key.
    /// </summary>
    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        return serviceType.IsInstanceOfType(this) ? this : null;
    }

    /// <summary>
    /// Disposes resources.
    /// </summary>
    public void Dispose()
    {
        // HttpClient is optionally provided, so we don't dispose it here
    }

    private ClaudeRequest BuildRequest(
        IList<ChatMessage> chatMessages,
        ChatOptions? options,
        bool stream)
    {
        var messages = new List<ClaudeMessage>();
        string? systemMessage = null;

        foreach (var message in chatMessages)
        {
            if (message.Role == ChatRole.System)
            {
                systemMessage = string.Join("\n", message.Contents.OfType<TextContent>().Select(c => c.Text));
            }
            else
            {
                var content = string.Join("\n", message.Contents.OfType<TextContent>().Select(c => c.Text));
                messages.Add(new ClaudeMessage
                {
                    Role = message.Role == ChatRole.User ? "user" : "assistant",
                    Content = content
                });
            }
        }

        return new ClaudeRequest
        {
            Model = _modelId,
            Messages = messages,
            System = systemMessage,
            MaxTokens = options?.MaxOutputTokens ?? 1024,
            Temperature = options?.Temperature,
            TopP = options?.TopP,
            Stream = stream
        };
    }

    private async Task<ClaudeResponse> SendRequestAsync(
        ClaudeRequest request,
        CancellationToken cancellationToken)
    {
        var token = await GetAuthTokenAsync(cancellationToken).ConfigureAwait(false);
        
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, _endpoint);
        httpRequest.Headers.Add("Authorization", $"Bearer {token}");
        httpRequest.Content = JsonContent.Create(request, options: JsonOptions);

        var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        httpResponse.EnsureSuccessStatusCode();

        var response = await httpResponse.Content.ReadFromJsonAsync<ClaudeResponse>(JsonOptions, cancellationToken).ConfigureAwait(false);
        return response ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    private async Task<Stream> SendStreamingRequestAsync(
        ClaudeRequest request,
        CancellationToken cancellationToken)
    {
        var token = await GetAuthTokenAsync(cancellationToken).ConfigureAwait(false);
        
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, _endpoint);
        httpRequest.Headers.Add("Authorization", $"Bearer {token}");
        httpRequest.Content = JsonContent.Create(request, options: JsonOptions);

        var httpResponse = await _httpClient.SendAsync(
            httpRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);
        
        httpResponse.EnsureSuccessStatusCode();
        return await httpResponse.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<string> GetAuthTokenAsync(CancellationToken cancellationToken)
    {
        var context = new TokenRequestContext(["https://cognitiveservices.azure.com/.default"]);
        var token = await _credential.GetTokenAsync(context, cancellationToken).ConfigureAwait(false);
        return token.Token;
    }

    private ChatCompletion ParseResponse(ClaudeResponse response)
    {
        var content = response.Content?.FirstOrDefault();
        var text = content?.Text ?? string.Empty;

        return new ChatCompletion(
            [new ChatMessage(ChatRole.Assistant, text)]
        )
        {
            CompletionId = response.Id,
            ModelId = response.Model,
            FinishReason = MapFinishReason(response.StopReason),
            Usage = new UsageDetails
            {
                InputTokenCount = response.Usage?.InputTokens,
                OutputTokenCount = response.Usage?.OutputTokens,
                TotalTokenCount = (response.Usage?.InputTokens ?? 0) + (response.Usage?.OutputTokens ?? 0)
            }
        };
    }

    private StreamingChatCompletionUpdate? ParseStreamingUpdate(string data)
    {
        try
        {
            var update = JsonSerializer.Deserialize<ClaudeStreamingEvent>(data, JsonOptions);
            
            if (update?.Type == "content_block_delta" && update.Delta?.Text != null)
            {
                return new StreamingChatCompletionUpdate
                {
                    Role = ChatRole.Assistant,
                    Text = update.Delta.Text
                };
            }
            
            if (update?.Type == "message_stop")
            {
                return new StreamingChatCompletionUpdate
                {
                    Role = ChatRole.Assistant,
                    FinishReason = ChatFinishReason.Stop
                };
            }
        }
        catch (JsonException)
        {
            // Ignore parsing errors for unsupported event types
            // Claude streaming API may include events we don't need to process
        }

        return null;
    }

    private ChatFinishReason? MapFinishReason(string? stopReason)
    {
        return stopReason switch
        {
            "end_turn" => ChatFinishReason.Stop,
            "max_tokens" => ChatFinishReason.Length,
            "stop_sequence" => ChatFinishReason.Stop,
            _ => null
        };
    }

    // Internal DTOs for Claude API
    private sealed class ClaudeRequest
    {
        public string Model { get; set; } = string.Empty;
        public List<ClaudeMessage> Messages { get; set; } = [];
        public string? System { get; set; }
        public int MaxTokens { get; set; }
        public double? Temperature { get; set; }
        public double? TopP { get; set; }
        public bool Stream { get; set; }
    }

    private sealed class ClaudeMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    private sealed class ClaudeResponse
    {
        public string? Id { get; set; }
        public string? Model { get; set; }
        public string? StopReason { get; set; }
        public List<ClaudeContent>? Content { get; set; }
        public ClaudeUsage? Usage { get; set; }
    }

    private sealed class ClaudeContent
    {
        public string? Type { get; set; }
        public string? Text { get; set; }
    }

    private sealed class ClaudeUsage
    {
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
    }

    private sealed class ClaudeStreamingEvent
    {
        public string? Type { get; set; }
        public ClaudeDelta? Delta { get; set; }
    }

    private sealed class ClaudeDelta
    {
        public string? Text { get; set; }
    }
}

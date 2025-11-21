using Azure.Identity;
using elbruno.Extensions.AI.Claude;
using Microsoft.Extensions.AI;

// Configuration - Replace with your actual values
var endpoint = new Uri(Environment.GetEnvironmentVariable("AZURE_CLAUDE_ENDPOINT") ?? "https://your-endpoint.cognitiveservices.azure.com");
var modelId = Environment.GetEnvironmentVariable("AZURE_CLAUDE_MODEL") ?? "claude-3-5-sonnet-20241022";

// Create the Azure Claude client
var credential = new DefaultAzureCredential();
var client = new AzureClaudeClient(endpoint, modelId, credential);

Console.WriteLine("Azure Claude Client Sample");
Console.WriteLine($"Endpoint: {endpoint}");
Console.WriteLine($"Model: {modelId}");
Console.WriteLine();

// Example 1: Simple completion
Console.WriteLine("=== Example 1: Simple Completion ===");
try
{
    var messages = new List<ChatMessage>
    {
        new(ChatRole.User, "What is the capital of France? Answer in one sentence.")
    };

    var response = await client.CompleteAsync(messages);
    Console.WriteLine($"Response: {response.Message.Text}");
    Console.WriteLine($"Tokens used: {response.Usage?.TotalTokenCount}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
Console.WriteLine();

// Example 2: Streaming completion
Console.WriteLine("=== Example 2: Streaming Completion ===");
try
{
    var messages = new List<ChatMessage>
    {
        new(ChatRole.User, "Write a short haiku about programming.")
    };

    Console.Write("Response: ");
    await foreach (var update in client.CompleteStreamingAsync(messages))
    {
        if (!string.IsNullOrEmpty(update.Text))
        {
            Console.Write(update.Text);
        }
    }
    Console.WriteLine();
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
Console.WriteLine();

// Example 3: Multi-turn conversation
Console.WriteLine("=== Example 3: Multi-turn Conversation ===");
try
{
    var conversation = new List<ChatMessage>
    {
        new(ChatRole.System, "You are a helpful assistant that provides concise answers."),
        new(ChatRole.User, "What is Azure?"),
    };

    var response1 = await client.CompleteAsync(conversation);
    Console.WriteLine($"User: What is Azure?");
    Console.WriteLine($"Assistant: {response1.Message.Text}");

    conversation.Add(response1.Message);
    conversation.Add(new ChatMessage(ChatRole.User, "What services does it provide?"));

    var response2 = await client.CompleteAsync(conversation);
    Console.WriteLine($"User: What services does it provide?");
    Console.WriteLine($"Assistant: {response2.Message.Text}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
Console.WriteLine();

Console.WriteLine("Sample completed. Press any key to exit.");
Console.ReadKey();


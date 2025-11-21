using elbruno.Extensions.AI.Claude;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

// Build configuration from Environment Variables and User Secrets
var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();

var endpoint = new Uri(configuration["AZURE_CLAUDE_ENDPOINT"] ?? throw new InvalidOperationException("Set AZURE_CLAUDE_ENDPOINT in user secrets or environment."));
var deploymentName = configuration["AZURE_CLAUDE_MODEL"] ?? throw new InvalidOperationException("Set AZURE_CLAUDE_MODEL in user secrets or environment.");
var apiKey = configuration["AZURE_CLAUDE_APIKEY"] ?? throw new InvalidOperationException("Set AZURE_CLAUDE_APIKEY in user secrets or environment.");

var client = new AzureClaudeClient(endpoint, deploymentName, apiKey);

Console.WriteLine("Azure Claude API Key Sample");
Console.WriteLine($"Endpoint: {endpoint}");
Console.WriteLine($"Deployment: {deploymentName}");
Console.WriteLine();

Console.WriteLine("=== Example: Simple Completion ===");

var messages = new List<ChatMessage>
{
    new(ChatRole.System, "You are a secure assistant that redacts secrets."),
    new(ChatRole.User, "Summarize the benefits of storing secrets in Azure Key Vault in two sentences."),
};

var response = await client.CompleteAsync(messages);
Console.WriteLine(response.Message.Text);

Console.WriteLine();
Console.WriteLine("Done. Store your API keys securely!");

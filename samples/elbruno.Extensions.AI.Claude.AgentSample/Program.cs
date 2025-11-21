using elbruno.Extensions.AI.Claude;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

// Build configuration from Environment Variables and User Secrets
var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();

// Configuration - Reads from User Secrets or Environment Variables
var endpoint = new Uri(configuration["AZURE_CLAUDE_ENDPOINT"] ?? throw new InvalidOperationException("Set AZURE_CLAUDE_ENDPOINT"));
var deploymentName = configuration["AZURE_CLAUDE_MODEL"] ?? "claude-sonnet-4-5";
var apiKey = configuration["AZURE_CLAUDE_APIKEY"] ?? throw new InvalidOperationException("Set AZURE_CLAUDE_APIKEY");

// Create the Azure Claude client
IChatClient chatClient = new AzureClaudeClient(endpoint, deploymentName, apiKey);

// Create the Agent
AIAgent writer = new ChatClientAgent(
    chatClient,
    new ChatClientAgentOptions
    {
        Name = "Writer",
        Instructions = "Write stories that are engaging and creative."
    });

Console.WriteLine("Agent created. Running...");

// Run the Agent
AgentRunResponse response = await writer.RunAsync("Write a short story about a haunted house.");
Console.WriteLine(response.Text);

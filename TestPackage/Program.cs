using elbruno.Extensions.AI.Claude;

// Test that the package can be imported and basic types are accessible
Console.WriteLine("Testing elbruno.Extensions.AI.Claude package...");

// Verify the AzureClaudeClient type is available
var clientType = typeof(AzureClaudeClient);
Console.WriteLine($"✓ AzureClaudeClient type found: {clientType.FullName}");

Console.WriteLine("✓ Package test successful!");

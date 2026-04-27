using System;
using System.IO;
using System.Text.Json;
using DiscordChatExporter.Mcp.Services;
using DiscordChatExporter.Mcp.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var token =
    Environment.GetEnvironmentVariable("DISCORD_TOKEN")
    ?? throw new InvalidOperationException(
        "DISCORD_TOKEN environment variable is not set. "
            + "Set it to your Discord authentication token before starting the MCP server."
    );

var settingsPath =
    Environment.GetEnvironmentVariable("DISCORD_MCP_SETTINGS")
    ?? Path.Combine(Directory.GetCurrentDirectory(), "mcp-settings.json");

if (!File.Exists(settingsPath))
    throw new InvalidOperationException(
        $"MCP settings file not found at '{settingsPath}'. "
            + "Create it based on mcp-settings.example.json in the repository root."
    );

var settings =
    JsonSerializer.Deserialize<McpSettings>(
        await File.ReadAllTextAsync(settingsPath),
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
    ) ?? throw new InvalidOperationException("Failed to parse mcp-settings.json.");

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddSingleton(settings);
builder.Services.AddSingleton(new DiscordService(token, settings));

builder.Services.AddMcpServer().WithStdioServerTransport().WithToolsFromAssembly();

await Console.Error.WriteLineAsync("Discord MCP server ready.");

await builder.Build().RunAsync();

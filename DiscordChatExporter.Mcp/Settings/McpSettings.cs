using System.Text.Json.Serialization;

namespace DiscordChatExporter.Mcp.Settings;

public class McpSettings
{
    [JsonPropertyName("allowedGuilds")]
    public string[] AllowedGuilds { get; init; } = [];

    [JsonPropertyName("allowedChannels")]
    public string[] AllowedChannels { get; init; } = [];

    [JsonPropertyName("allowAllChannelsInGuilds")]
    public bool AllowAllChannelsInGuilds { get; init; } = false;

    [JsonPropertyName("exportPath")]
    public string? ExportPath { get; init; }

    [JsonPropertyName("viewerPort")]
    public int ViewerPort { get; init; } = 5722;
}

using System.Text.Json.Serialization;

namespace DiscordChatExporter.Mcp.Settings;

public class McpSettings
{
    [JsonPropertyName("allowedGuilds")]
    public string[] AllowedGuilds { get; init; } = [];

    [JsonPropertyName("allowedChannels")]
    public string[] AllowedChannels { get; init; } = [];

    /// <summary>
    /// When true, all channels in every allowed guild are accessible.
    /// Use guildsAllowingAllChannels for per-guild granularity instead.
    /// </summary>
    [JsonPropertyName("allowAllChannelsInGuilds")]
    public bool AllowAllChannelsInGuilds { get; init; } = false;

    /// <summary>
    /// Guild IDs whose channels are all accessible, regardless of allowedChannels.
    /// More precise than allowAllChannelsInGuilds — lets you lock down specific guilds (e.g. DMs)
    /// while leaving others fully open.
    /// </summary>
    [JsonPropertyName("guildsAllowingAllChannels")]
    public string[] GuildsAllowingAllChannels { get; init; } = [];

    [JsonPropertyName("exportPath")]
    public string? ExportPath { get; init; }

    [JsonPropertyName("viewerPort")]
    public int ViewerPort { get; init; } = 5722;

    /// <summary>
    /// When true, bypasses all allowlist filtering so you can discover guild and channel IDs.
    /// Set to false (or remove) once you've populated allowedGuilds / allowedChannels.
    /// </summary>
    [JsonPropertyName("discoveryMode")]
    public bool DiscoveryMode { get; init; } = false;
}

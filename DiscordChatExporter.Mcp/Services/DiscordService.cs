using System;
using System.IO;
using System.Linq;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Mcp.Settings;

namespace DiscordChatExporter.Mcp.Services;

public class DiscordService
{
    public DiscordClient Client { get; }
    public McpSettings Settings { get; }

    public DiscordService(string token, McpSettings settings)
    {
        Client = new DiscordClient(token);
        Settings = settings;
    }

    public bool IsGuildAllowed(string guildId) =>
        Settings.DiscoveryMode
        || Settings.AllowedGuilds.Contains(guildId, StringComparer.OrdinalIgnoreCase);

    public bool IsChannelAllowed(string channelId, string guildId)
    {
        if (!IsGuildAllowed(guildId))
            return false;

        return Settings.DiscoveryMode
            || Settings.AllowAllChannelsInGuilds
            || Settings.GuildsAllowingAllChannels.Contains(guildId, StringComparer.OrdinalIgnoreCase)
            || Settings.AllowedChannels.Contains(channelId, StringComparer.OrdinalIgnoreCase);
    }

    public string GetExportBasePath()
    {
        var fromEnv = Environment.GetEnvironmentVariable("DISCORD_EXPORT_PATH");
        if (!string.IsNullOrWhiteSpace(fromEnv))
            return fromEnv;

        if (!string.IsNullOrWhiteSpace(Settings.ExportPath))
            return Settings.ExportPath;

        return Path.Combine(Directory.GetCurrentDirectory(), "discord-exports");
    }
}

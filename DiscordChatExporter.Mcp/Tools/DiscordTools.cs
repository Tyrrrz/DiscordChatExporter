using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exceptions;
using DiscordChatExporter.Core.Exporting;
using DiscordChatExporter.Core.Exporting.Filtering;
using DiscordChatExporter.Core.Exporting.Partitioning;
using DiscordChatExporter.Mcp.Services;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace DiscordChatExporter.Mcp.Tools;

[McpServerToolType]
public class DiscordTools(DiscordService discord)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private static CallToolResult Ok(object data) =>
        new()
        {
            Content = [new TextContentBlock { Text = JsonSerializer.Serialize(data, JsonOptions) }],
            IsError = false,
        };

    private static CallToolResult Error(string message) =>
        new() { Content = [new TextContentBlock { Text = message }], IsError = true };

    [McpServerTool(Name = "list_guilds")]
    [Description(
        "List all Discord servers accessible with the configured token, filtered to the allowlist."
    )]
    public async Task<CallToolResult> ListGuildsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var guilds = new List<object>();
            await foreach (var guild in discord.Client.GetUserGuildsAsync(cancellationToken))
            {
                if (discord.IsGuildAllowed(guild.Id.ToString()))
                    guilds.Add(new { id = guild.Id.ToString(), name = guild.Name });
            }

            return Ok(guilds);
        }
        catch (DiscordChatExporterException ex)
        {
            return Error(ex.Message);
        }
    }

    [McpServerTool(Name = "list_channels")]
    [Description(
        "List channels in a Discord server. Only returns channels permitted by the allowlist."
    )]
    public async Task<CallToolResult> ListChannelsAsync(
        [Description("Discord server (guild) ID")] string guildId,
        CancellationToken cancellationToken
    )
    {
        if (!discord.IsGuildAllowed(guildId))
            return Error($"Guild '{guildId}' is not in the configured allowlist.");

        try
        {
            var snowflake = Snowflake.Parse(guildId);
            var channels = new List<object>();

            await foreach (
                var channel in discord.Client.GetGuildChannelsAsync(snowflake, cancellationToken)
            )
            {
                if (!discord.IsChannelAllowed(channel.Id.ToString(), guildId))
                    continue;

                channels.Add(
                    new
                    {
                        id = channel.Id.ToString(),
                        name = channel.Name,
                        kind = channel.Kind.ToString(),
                        topic = channel.Topic,
                        category = channel.Parent?.Name,
                        position = channel.Position,
                        isThread = channel.IsThread,
                        isEmpty = channel.IsEmpty,
                    }
                );
            }

            return Ok(channels);
        }
        catch (FormatException)
        {
            return Error($"'{guildId}' is not a valid Discord ID.");
        }
        catch (DiscordChatExporterException ex)
        {
            return Error(ex.Message);
        }
    }

    [McpServerTool(Name = "list_threads")]
    [Description(
        "List active and archived threads in a Discord channel. "
            + "Threads are separate conversation spaces within a channel."
    )]
    public async Task<CallToolResult> ListThreadsAsync(
        [Description("Discord channel ID")] string channelId,
        [Description("Include archived threads (default: true)")] bool includeArchived = true,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var snowflake = Snowflake.Parse(channelId);
            var channel = await discord.Client.GetChannelAsync(snowflake, cancellationToken);

            if (!discord.IsChannelAllowed(channelId, channel.GuildId.ToString()))
                return Error($"Channel '{channelId}' is not in the configured allowlist.");

            var threads = new List<object>();
            await foreach (
                var thread in discord.Client.GetChannelThreadsAsync(
                    [channel],
                    includeArchived,
                    cancellationToken: cancellationToken
                )
            )
            {
                threads.Add(
                    new
                    {
                        id = thread.Id.ToString(),
                        name = thread.Name,
                        isArchived = thread.IsArchived,
                        isEmpty = thread.IsEmpty,
                        lastMessageId = thread.LastMessageId?.ToString(),
                    }
                );
            }

            return Ok(threads);
        }
        catch (FormatException)
        {
            return Error($"'{channelId}' is not a valid Discord ID.");
        }
        catch (DiscordChatExporterException ex)
        {
            return Error(ex.Message);
        }
    }

    [McpServerTool(Name = "get_channel_info")]
    [Description(
        "Get metadata about a Discord channel: name, topic, category, type, and last message info."
    )]
    public async Task<CallToolResult> GetChannelInfoAsync(
        [Description("Discord channel ID")] string channelId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var snowflake = Snowflake.Parse(channelId);
            var channel = await discord.Client.GetChannelAsync(snowflake, cancellationToken);

            if (!discord.IsChannelAllowed(channelId, channel.GuildId.ToString()))
                return Error($"Channel '{channelId}' is not in the configured allowlist.");

            return Ok(
                new
                {
                    id = channel.Id.ToString(),
                    name = channel.Name,
                    kind = channel.Kind.ToString(),
                    topic = channel.Topic,
                    category = channel.Parent?.Name,
                    categoryId = channel.Parent?.Id.ToString(),
                    guildId = channel.GuildId.ToString(),
                    position = channel.Position,
                    isThread = channel.IsThread,
                    isArchived = channel.IsArchived,
                    isEmpty = channel.IsEmpty,
                    lastMessageId = channel.LastMessageId?.ToString(),
                }
            );
        }
        catch (FormatException)
        {
            return Error($"'{channelId}' is not a valid Discord ID.");
        }
        catch (DiscordChatExporterException ex)
        {
            return Error(ex.Message);
        }
    }

    [McpServerTool(Name = "get_messages")]
    [Description(
        "Fetch messages from a Discord channel or thread. "
            + "Use 'after' and 'before' (message Snowflake IDs) for pagination. "
            + "Returns up to 'limit' messages (max 100, default 50)."
    )]
    public async Task<CallToolResult> GetMessagesAsync(
        [Description("Discord channel or thread ID")] string channelId,
        [Description("Return messages after this message ID (exclusive)")] string? after = null,
        [Description("Return messages before this message ID (exclusive)")] string? before = null,
        [Description("Maximum number of messages to return (default 50, max 100)")] int limit = 50,
        CancellationToken cancellationToken = default
    )
    {
        if (limit is < 1 or > 100)
            return Error("Limit must be between 1 and 100.");

        try
        {
            var snowflake = Snowflake.Parse(channelId);
            var channel = await discord.Client.GetChannelAsync(snowflake, cancellationToken);

            if (!discord.IsChannelAllowed(channelId, channel.GuildId.ToString()))
                return Error($"Channel '{channelId}' is not in the configured allowlist.");

            Snowflake? afterSnowflake = after is not null
                ? Snowflake.Parse(after)
                : (Snowflake?)null;
            Snowflake? beforeSnowflake = before is not null
                ? Snowflake.Parse(before)
                : (Snowflake?)null;

            var messages = new List<object>();
            await foreach (
                var message in discord.Client.GetMessagesAsync(
                    snowflake,
                    afterSnowflake,
                    beforeSnowflake,
                    cancellationToken: cancellationToken
                )
            )
            {
                messages.Add(ProjectMessage(message));
                if (messages.Count >= limit)
                    break;
            }

            return Ok(messages);
        }
        catch (FormatException)
        {
            return Error($"One or more provided IDs are not valid Discord Snowflakes.");
        }
        catch (DiscordChatExporterException ex)
        {
            return Error(ex.Message);
        }
    }

    [McpServerTool(Name = "export_channel")]
    [Description(
        "Export a Discord channel's message history to a local file. "
            + "Supported formats: Json (default), HtmlDark, HtmlLight, PlainText, Csv. "
            + "Returns the path to the exported file."
    )]
    public async Task<CallToolResult> ExportChannelAsync(
        [Description("Discord channel or thread ID")] string channelId,
        [Description("Export format: Json, HtmlDark, HtmlLight, PlainText, Csv")]
            string format = "Json",
        CancellationToken cancellationToken = default
    )
    {
        if (!Enum.TryParse<ExportFormat>(format, ignoreCase: true, out var exportFormat))
            return Error(
                $"Unknown format '{format}'. Valid values: Json, HtmlDark, HtmlLight, PlainText, Csv."
            );

        try
        {
            var snowflake = Snowflake.Parse(channelId);
            var channel = await discord.Client.GetChannelAsync(snowflake, cancellationToken);

            if (!discord.IsChannelAllowed(channelId, channel.GuildId.ToString()))
                return Error($"Channel '{channelId}' is not in the configured allowlist.");

            var guild = await discord.Client.GetGuildAsync(channel.GuildId, cancellationToken);

            var outputDir = Path.Combine(
                discord.GetExportBasePath(),
                SanitizePath(guild.Name),
                SanitizePath(channel.GetHierarchicalName())
            );

            Directory.CreateDirectory(outputDir);

            var request = new ExportRequest(
                guild,
                channel,
                outputDir + Path.DirectorySeparatorChar,
                null,
                exportFormat,
                null,
                null,
                PartitionLimit.Null,
                MessageFilter.Null,
                false,
                true,
                false,
                false,
                "en-US",
                true
            );

            var exporter = new ChannelExporter(discord.Client);
            await exporter.ExportChannelAsync(request, cancellationToken: cancellationToken);

            return Ok(
                new
                {
                    outputDir,
                    outputFile = request.OutputFilePath,
                    guild = guild.Name,
                    channel = channel.GetHierarchicalName(),
                    format = exportFormat.ToString(),
                }
            );
        }
        catch (ChannelEmptyException ex)
        {
            return Ok(new { warning = ex.Message });
        }
        catch (FormatException)
        {
            return Error($"'{channelId}' is not a valid Discord ID.");
        }
        catch (DiscordChatExporterException ex)
        {
            return Error(ex.Message);
        }
    }

    private static object ProjectMessage(Message message) =>
        new
        {
            id = message.Id.ToString(),
            timestamp = message.Timestamp,
            editedTimestamp = message.EditedTimestamp,
            isPinned = message.IsPinned,
            author = new
            {
                id = message.Author.Id.ToString(),
                name = message.Author.Name,
                displayName = message.Author.DisplayName,
                isBot = message.Author.IsBot,
            },
            content = message.Content,
            attachments = message.Attachments.Select(a => new
            {
                url = a.Url,
                fileName = a.FileName,
                fileSizeBytes = a.FileSize.TotalBytes,
                isImage = a.IsImage,
                isVideo = a.IsVideo,
            }),
            embeds = message.Embeds.Select(e => new
            {
                e.Title,
                e.Description,
                e.Url,
            }),
            reactions = message.Reactions.Select(r => new
            {
                emoji = r.Emoji.Name,
                count = r.Count,
            }),
            replyToId = message.Reference?.MessageId?.ToString(),
            mentionedUsers = message.MentionedUsers.Select(u => new
            {
                id = u.Id.ToString(),
                name = u.Name,
            }),
        };

    private static string SanitizePath(string name) =>
        string.Concat(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
}

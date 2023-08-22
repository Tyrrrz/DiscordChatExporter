using System.Collections.Generic;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exceptions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Cli.Commands;

[Command("exportall", Description = "Exports all accessible channels.")]
public class ExportAllCommand : ExportCommandBase
{
    [CommandOption("include-dm", Description = "Include direct message channels.")]
    public bool IncludeDirectChannels { get; init; } = true;

    [CommandOption("include-guilds", Description = "Include guild channels.")]
    public bool IncludeGuildChannels { get; init; } = true;

    [CommandOption("include-vc", Description = "Include voice channels.")]
    public bool IncludeVoiceChannels { get; init; } = true;

    [CommandOption("include-threads", Description = "Include threads.")]
    public bool IncludeThreads { get; init; } = false;

    [CommandOption("include-archived-threads", Description = "Include archived threads.")]
    public bool IncludeArchivedThreads { get; init; } = false;

    [CommandOption(
        "data-package",
        Description = "Path to the personal data package (ZIP file) requested from Discord. "
            + "If provided, only channels referenced in the dump will be exported."
    )]
    public string? DataPackageFilePath { get; init; }

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        await base.ExecuteAsync(console);

        // Cannot include archived threads without including active threads as well
        if (IncludeArchivedThreads && !IncludeThreads)
        {
            throw new CommandException(
                "Option --include-archived-threads can only be used when --include-threads is also specified."
            );
        }

        var cancellationToken = console.RegisterCancellationHandler();
        var channels = new List<Channel>();

        // Pull from the API
        if (string.IsNullOrWhiteSpace(DataPackageFilePath))
        {
            await console.Output.WriteLineAsync("Fetching channels...");

            await foreach (var guild in Discord.GetUserGuildsAsync(cancellationToken))
            {
                // Regular channels
                await foreach (
                    var channel in Discord.GetGuildChannelsAsync(guild.Id, cancellationToken)
                )
                {
                    if (channel.Kind == ChannelKind.GuildCategory)
                        continue;

                    if (!IncludeVoiceChannels && channel.Kind.IsVoice())
                        continue;

                    channels.Add(channel);
                }

                // Threads
                if (IncludeThreads)
                {
                    await foreach (
                        var thread in Discord.GetGuildThreadsAsync(
                            guild.Id,
                            IncludeArchivedThreads,
                            cancellationToken
                        )
                    )
                    {
                        channels.Add(thread);
                    }
                }
            }
        }
        // Pull from the data package
        else
        {
            await console.Output.WriteLineAsync("Extracting channels...");
            using var archive = ZipFile.OpenRead(DataPackageFilePath);

            var entry = archive.GetEntry("messages/index.json");
            if (entry is null)
                throw new CommandException("Could not find channel index inside the data package.");

            await using var stream = entry.Open();
            using var document = await JsonDocument.ParseAsync(stream, default, cancellationToken);

            foreach (var property in document.RootElement.EnumerateObjectOrEmpty())
            {
                var channelId = Snowflake.Parse(property.Name);
                var channelName = property.Value.GetString();

                // Null items refer to deleted channels
                if (channelName is null)
                    continue;

                await console.Output.WriteLineAsync(
                    $"Fetching channel '{channelName}' ({channelId})..."
                );

                try
                {
                    var channel = await Discord.GetChannelAsync(channelId, cancellationToken);
                    channels.Add(channel);
                }
                catch (DiscordChatExporterException)
                {
                    await console.Error.WriteLineAsync(
                        $"Channel '{channelName}' ({channelId}) is inaccessible."
                    );
                }
            }
        }

        // Filter out unwanted channels
        if (!IncludeDirectChannels)
            channels.RemoveAll(c => c.Kind.IsDirect());
        if (!IncludeGuildChannels)
            channels.RemoveAll(c => c.Kind.IsGuild());
        if (!IncludeVoiceChannels)
            channels.RemoveAll(c => c.Kind.IsVoice());
        if (!IncludeThreads)
            channels.RemoveAll(c => c.Kind.IsThread());
        if (!IncludeArchivedThreads)
            channels.RemoveAll(c => c.Kind.IsThread() && c.IsArchived);

        await ExportAsync(console, channels);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Cli.Commands.Converters;
using DiscordChatExporter.Cli.Commands.Shared;
using DiscordChatExporter.Cli.Utils.Extensions;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Discord.Dump;
using DiscordChatExporter.Core.Exceptions;
using Spectre.Console;

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

    [CommandOption(
        "include-threads",
        Description = "Which types of threads should be included.",
        Converter = typeof(ThreadInclusionModeBindingConverter)
    )]
    public ThreadInclusionMode ThreadInclusionMode { get; init; } = ThreadInclusionMode.None;

    [CommandOption(
        "data-package",
        Description = "Path to the personal data package (ZIP file) requested from Discord. "
            + "If provided, only channels referenced in the dump will be exported."
    )]
    public string? DataPackageFilePath { get; init; }

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        await base.ExecuteAsync(console);

        var cancellationToken = console.RegisterCancellationHandler();
        var channels = new List<Channel>();

        // Pull from the API
        if (string.IsNullOrWhiteSpace(DataPackageFilePath))
        {
            await foreach (var guild in Discord.GetUserGuildsAsync(cancellationToken))
            {
                // Regular channels
                await console
                    .Output
                    .WriteLineAsync($"Fetching channels for guild '{guild.Name}'...");

                var fetchedChannelsCount = 0;
                await console
                    .CreateStatusTicker()
                    .StartAsync(
                        "...",
                        async ctx =>
                        {
                            await foreach (
                                var channel in Discord.GetGuildChannelsAsync(
                                    guild.Id,
                                    cancellationToken
                                )
                            )
                            {
                                if (channel.IsCategory)
                                    continue;

                                if (!IncludeVoiceChannels && channel.IsVoice)
                                    continue;

                                channels.Add(channel);

                                ctx.Status(
                                    Markup.Escape($"Fetched '{channel.GetHierarchicalName()}'.")
                                );

                                fetchedChannelsCount++;
                            }
                        }
                    );

                await console.Output.WriteLineAsync($"Fetched {fetchedChannelsCount} channel(s).");

                // Threads
                if (ThreadInclusionMode != ThreadInclusionMode.None)
                {
                    await console
                        .Output
                        .WriteLineAsync($"Fetching threads for guild '{guild.Name}'...");

                    var fetchedThreadsCount = 0;
                    await console
                        .CreateStatusTicker()
                        .StartAsync(
                            "...",
                            async ctx =>
                            {
                                await foreach (
                                    var thread in Discord.GetGuildThreadsAsync(
                                        guild.Id,
                                        ThreadInclusionMode == ThreadInclusionMode.All,
                                        Before,
                                        After,
                                        cancellationToken
                                    )
                                )
                                {
                                    channels.Add(thread);

                                    ctx.Status(
                                        Markup.Escape($"Fetched '{thread.GetHierarchicalName()}'.")
                                    );

                                    fetchedThreadsCount++;
                                }
                            }
                        );

                    await console
                        .Output
                        .WriteLineAsync($"Fetched {fetchedThreadsCount} thread(s).");
                }
            }
        }
        // Pull from the data package
        else
        {
            await console.Output.WriteLineAsync("Extracting channels...");

            var dump = await DataDump.LoadAsync(DataPackageFilePath, cancellationToken);
            var inaccessibleChannels = new List<DataDumpChannel>();

            await console
                .CreateStatusTicker()
                .StartAsync(
                    "...",
                    async ctx =>
                    {
                        foreach (var dumpChannel in dump.Channels)
                        {
                            ctx.Status(
                                Markup.Escape(
                                    $"Fetching '{dumpChannel.Name}' ({dumpChannel.Id})..."
                                )
                            );

                            try
                            {
                                var channel = await Discord.GetChannelAsync(
                                    dumpChannel.Id,
                                    cancellationToken
                                );

                                channels.Add(channel);
                            }
                            catch (DiscordChatExporterException)
                            {
                                inaccessibleChannels.Add(dumpChannel);
                            }
                        }
                    }
                );

            await console.Output.WriteLineAsync($"Fetched {channels} channel(s).");

            // Print inaccessible channels
            if (inaccessibleChannels.Any())
            {
                await console.Output.WriteLineAsync();

                using (console.WithForegroundColor(ConsoleColor.Red))
                {
                    await console
                        .Error
                        .WriteLineAsync("Failed to access the following channel(s):");
                }

                foreach (var dumpChannel in inaccessibleChannels)
                    await console.Error.WriteLineAsync($"{dumpChannel.Name} ({dumpChannel.Id})");

                await console.Error.WriteLineAsync();
            }
        }

        // Filter out unwanted channels
        if (!IncludeDirectChannels)
            channels.RemoveAll(c => c.IsDirect);
        if (!IncludeGuildChannels)
            channels.RemoveAll(c => c.IsGuild);
        if (!IncludeVoiceChannels)
            channels.RemoveAll(c => c.IsVoice);
        if (ThreadInclusionMode == ThreadInclusionMode.None)
            channels.RemoveAll(c => c.IsThread);
        if (ThreadInclusionMode != ThreadInclusionMode.All)
            channels.RemoveAll(c => c is { IsThread: true, IsArchived: true });

        await ExportAsync(console, channels);
    }
}

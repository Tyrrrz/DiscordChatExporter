using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Cli.Commands.Converters;
using DiscordChatExporter.Cli.Commands.Shared;
using DiscordChatExporter.Cli.Utils.Extensions;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using Spectre.Console;

namespace DiscordChatExporter.Cli.Commands;

[Command("exportguild", Description = "Exports all channels within the specified server.")]
public class ExportGuildCommand : ExportCommandBase
{
    [CommandOption("guild", 'g', Description = "Server ID.")]
    public required Snowflake GuildId { get; init; }

    [CommandOption("include-vc", Description = "Include voice channels.")]
    public bool IncludeVoiceChannels { get; init; } = true;

    [CommandOption(
        "include-threads",
        Description = "Which types of threads should be included.",
        Converter = typeof(ThreadInclusionModeBindingConverter)
    )]
    public ThreadInclusionMode ThreadInclusionMode { get; init; } = ThreadInclusionMode.None;

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        await base.ExecuteAsync(console);

        var cancellationToken = console.RegisterCancellationHandler();
        var channels = new List<Channel>();

        // Regular channels
        await console.Output.WriteLineAsync("Fetching channels...");

        var fetchedChannelsCount = 0;
        await console
            .CreateStatusTicker()
            .StartAsync(
                "...",
                async ctx =>
                {
                    await foreach (
                        var channel in Discord.GetGuildChannelsAsync(GuildId, cancellationToken)
                    )
                    {
                        if (channel.IsCategory)
                            continue;

                        if (!IncludeVoiceChannels && channel.IsVoice)
                            continue;

                        channels.Add(channel);

                        ctx.Status(Markup.Escape($"Fetched '{channel.GetHierarchicalName()}'."));

                        fetchedChannelsCount++;
                    }
                }
            );

        await console.Output.WriteLineAsync($"Fetched {fetchedChannelsCount} channel(s).");

        // Threads
        if (ThreadInclusionMode != ThreadInclusionMode.None)
        {
            await console.Output.WriteLineAsync("Fetching threads...");

            var fetchedThreadsCount = 0;
            await console
                .CreateStatusTicker()
                .StartAsync(
                    "...",
                    async ctx =>
                    {
                        await foreach (
                            var thread in Discord.GetGuildThreadsAsync(
                                GuildId,
                                ThreadInclusionMode == ThreadInclusionMode.All,
                                Before,
                                After,
                                cancellationToken
                            )
                        )
                        {
                            channels.Add(thread);

                            ctx.Status(Markup.Escape($"Fetched '{thread.GetHierarchicalName()}'."));

                            fetchedThreadsCount++;
                        }
                    }
                );

            await console.Output.WriteLineAsync($"Fetched {fetchedThreadsCount} thread(s).");
        }

        await ExportAsync(console, channels);
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Cli.Commands.Converters;
using DiscordChatExporter.Cli.Commands.Shared;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using Spectre.Console;

namespace DiscordChatExporter.Cli.Commands;

[Command("exportguild", Description = "Exports all channels within the specified guild.")]
public class ExportGuildCommand : ExportCommandBase
{
    [CommandOption("guild", 'g', Description = "Guild ID.")]
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

        await console.Output.WriteLineAsync("Fetching channels...");

        // Regular channels
        await foreach (var channel in Discord.GetGuildChannelsAsync(GuildId, cancellationToken))
        {
            if (channel.IsCategory)
                continue;

            if (!IncludeVoiceChannels && channel.IsVoice)
                continue;

            channels.Add(channel);
        }

        await console.Output.WriteLineAsync($"  Found {channels.Count} channels.");

        // Threads
        if (ThreadInclusionMode != ThreadInclusionMode.None)
        {
            AnsiConsole.MarkupLine("Fetching threads...");
            await AnsiConsole
                .Status()
                .StartAsync(
                    "Found 0 threads.",
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
                            ctx.Status(
                                $"Found {channels.Count(channel => channel.IsThread)} threads: {thread.GetHierarchicalName()}"
                            );
                        }
                    }
                );
            await console.Output.WriteLineAsync(
                $"  Found {channels.Count(channel => channel.IsThread)} threads."
            );
        }

        await ExportAsync(console, channels);
    }
}

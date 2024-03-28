using System;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Cli.Commands.Converters;
using DiscordChatExporter.Cli.Commands.Shared;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Cli.Commands;

[Command("channels", Description = "Get the list of channels in a server.")]
public class GetChannelsCommand : DiscordCommandBase
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

        var channels = (await Discord.GetGuildChannelsAsync(GuildId, cancellationToken))
            .Where(c => !c.IsCategory)
            .Where(c => IncludeVoiceChannels || !c.IsVoice)
            .OrderBy(c => c.Parent?.Position)
            .ThenBy(c => c.Name)
            .ToArray();

        var channelIdMaxLength = channels
            .Select(c => c.Id.ToString().Length)
            .OrderDescending()
            .FirstOrDefault();

        var threads =
            ThreadInclusionMode != ThreadInclusionMode.None
                ? (
                    await Discord.GetGuildThreadsAsync(
                        GuildId,
                        ThreadInclusionMode == ThreadInclusionMode.All,
                        null,
                        null,
                        cancellationToken
                    )
                )
                    .OrderBy(c => c.Name)
                    .ToArray()
                : [];

        foreach (var channel in channels)
        {
            // Channel ID
            await console.Output.WriteAsync(
                channel.Id.ToString().PadRight(channelIdMaxLength, ' ')
            );

            // Separator
            using (console.WithForegroundColor(ConsoleColor.DarkGray))
                await console.Output.WriteAsync(" | ");

            // Channel name
            using (console.WithForegroundColor(ConsoleColor.White))
                await console.Output.WriteLineAsync(channel.GetHierarchicalName());

            var channelThreads = threads.Where(t => t.Parent?.Id == channel.Id).ToArray();
            var channelThreadIdMaxLength = channelThreads
                .Select(t => t.Id.ToString().Length)
                .OrderDescending()
                .FirstOrDefault();

            foreach (var channelThread in channelThreads)
            {
                // Indent
                await console.Output.WriteAsync(" * ");

                // Thread ID
                await console.Output.WriteAsync(
                    channelThread.Id.ToString().PadRight(channelThreadIdMaxLength, ' ')
                );

                // Separator
                using (console.WithForegroundColor(ConsoleColor.DarkGray))
                    await console.Output.WriteAsync(" | ");

                // Thread name
                using (console.WithForegroundColor(ConsoleColor.White))
                    await console.Output.WriteAsync($"Thread / {channelThread.Name}");

                // Separator
                using (console.WithForegroundColor(ConsoleColor.DarkGray))
                    await console.Output.WriteAsync(" | ");

                // Thread status
                using (console.WithForegroundColor(ConsoleColor.White))
                    await console.Output.WriteLineAsync(
                        channelThread.IsArchived ? "Archived" : "Active"
                    );
            }
        }
    }
}

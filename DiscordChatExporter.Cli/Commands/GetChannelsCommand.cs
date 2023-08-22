using System;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Cli.Commands;

[Command("channels", Description = "Get the list of channels in a guild.")]
public class GetChannelsCommand : DiscordCommandBase
{
    [CommandOption("guild", 'g', Description = "Guild ID.")]
    public required Snowflake GuildId { get; init; }

    [CommandOption("include-vc", Description = "Include voice channels.")]
    public bool IncludeVoiceChannels { get; init; } = true;

    [CommandOption("include-threads", Description = "Include threads.")]
    public bool IncludeThreads { get; init; } = false;

    [CommandOption("include-archived-threads", Description = "Include archived threads.")]
    public bool IncludeArchivedThreads { get; init; } = false;

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

        var channels = (await Discord.GetGuildChannelsAsync(GuildId, cancellationToken))
            .Where(c => c.Kind != ChannelKind.GuildCategory)
            .Where(c => IncludeVoiceChannels || !c.Kind.IsVoice())
            .OrderBy(c => c.Parent?.Position)
            .ThenBy(c => c.Name)
            .ToArray();

        var channelIdMaxLength = channels
            .Select(c => c.Id.ToString().Length)
            .OrderDescending()
            .FirstOrDefault();

        var threads = IncludeThreads
            ? (
                await Discord.GetGuildThreadsAsync(
                    GuildId,
                    IncludeArchivedThreads,
                    cancellationToken
                )
            )
                .OrderBy(c => c.Name)
                .ToArray()
            : Array.Empty<Channel>();

        foreach (var channel in channels)
        {
            // Channel ID
            await console.Output.WriteAsync(
                channel.Id.ToString().PadRight(channelIdMaxLength, ' ')
            );

            // Separator
            using (console.WithForegroundColor(ConsoleColor.DarkGray))
                await console.Output.WriteAsync(" | ");

            // Channel category / name
            using (console.WithForegroundColor(ConsoleColor.White))
                await console.Output.WriteLineAsync($"{channel.Category} / {channel.Name}");

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

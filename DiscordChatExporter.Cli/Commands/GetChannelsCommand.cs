using System;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Cli.Commands;

[Command("channels", Description = "Get the list of channels in a guild.")]
public class GetChannelsCommand : DiscordCommandBase
{
    [CommandOption(
        "guild",
        'g',
        Description = "Guild ID."
    )]
    public required Snowflake GuildId { get; init; }

    [CommandOption(
        "include-threads",
        Description = "Include threads in the output."
    )]
    public bool IncludeThreads { get; init; }

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        var cancellationToken = console.RegisterCancellationHandler();

        var channels = (await Discord.GetGuildChannelsAsync(GuildId, cancellationToken))
            .Where(c => c.Kind != ChannelKind.GuildCategory)
            .OrderBy(c => c.Category.Position)
            .ThenBy(c => c.Name)
            .ToArray();

        var threads = IncludeThreads
            ? (await Discord.GetGuildThreadsAsync(GuildId, cancellationToken))
            .OrderBy(c => c.Name)
            .ToArray()
            : Array.Empty<ChannelThread>();

        foreach (var channel in channels)
        {
            // Channel ID
            await console.Output.WriteAsync(
                channel.Id.ToString().PadRight(18, ' ')
            );

            // Separator
            using (console.WithForegroundColor(ConsoleColor.DarkGray))
                await console.Output.WriteAsync(" | ");

            // Channel category / name
            using (console.WithForegroundColor(ConsoleColor.White))
                await console.Output.WriteLineAsync($"{channel.Category.Name} / {channel.Name}");

            foreach (var thread in threads.Where(t => t.ParentId == channel.Id))
            {
                // Indent
                await console.Output.WriteAsync('\t');

                // Thread ID
                await console.Output.WriteAsync(
                    thread.Id.ToString().PadRight(18, ' ')
                );

                // Separator
                using (console.WithForegroundColor(ConsoleColor.DarkGray))
                    await console.Output.WriteAsync(" | ");

                // Thread name
                using (console.WithForegroundColor(ConsoleColor.White))
                    await console.Output.WriteLineAsync($"Thread / {thread.Name}");
            }
        }
    }
}
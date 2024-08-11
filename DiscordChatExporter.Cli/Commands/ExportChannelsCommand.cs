using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Cli.Commands.Converters;
using DiscordChatExporter.Cli.Commands.Shared;
using DiscordChatExporter.Cli.Utils.Extensions;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Utils.Extensions;
using Spectre.Console;

namespace DiscordChatExporter.Cli.Commands;

[Command("export", Description = "Exports one or multiple channels.")]
public class ExportChannelsCommand : ExportCommandBase
{
    // TODO: change this to plural (breaking change)
    [CommandOption(
        "channel",
        'c',
        Description = "Channel ID(s). "
            + "If provided with category ID(s), all channels inside those categories will be exported."
    )]
    public required IReadOnlyList<Snowflake> ChannelIds { get; init; }

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

        await console.Output.WriteLineAsync("Resolving channel(s)...");

        var channels = new List<Channel>();
        var channelsByGuild = new Dictionary<Snowflake, IReadOnlyList<Channel>>();

        foreach (var channelId in ChannelIds)
        {
            var channel = await Discord.GetChannelAsync(channelId, cancellationToken);

            // Unwrap categories
            if (channel.IsCategory)
            {
                var guildChannels =
                    channelsByGuild.GetValueOrDefault(channel.GuildId)
                    ?? await Discord.GetGuildChannelsAsync(channel.GuildId, cancellationToken);

                foreach (var guildChannel in guildChannels)
                {
                    if (guildChannel.Parent?.Id == channel.Id)
                        channels.Add(guildChannel);
                }

                // Cache the guild channels to avoid redundant work
                channelsByGuild[channel.GuildId] = guildChannels;
            }
            else
            {
                channels.Add(channel);
            }
        }

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
                            var thread in Discord.GetChannelThreadsAsync(
                                channels.ToArray(),
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

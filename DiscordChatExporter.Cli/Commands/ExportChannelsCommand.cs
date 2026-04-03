using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Cli.Utils.Extensions;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Cli.Commands;

[Command("export", Description = "Exports one or multiple channels.")]
public partial class ExportChannelsCommand : ExportCommandBase
{
    [CommandParameter(
        0,
        Name = "channel-ids",
        Description = "Channel ID(s). "
            + "If provided with category ID(s), all channels inside those categories will be exported. "
            + "If not provided, channel IDs are read from standard input (one per line), "
            + "enabling piping from the 'list channels' or 'list channels dm' commands."
    )]
    public IReadOnlyList<Snowflake> ChannelIds { get; set; } = [];

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        await base.ExecuteAsync(console);

        var cancellationToken = console.RegisterCancellationHandler();

        // If no channel IDs were specified, read them from stdin
        var channelIds = new List<Snowflake>(ChannelIds);
        if (channelIds.Count == 0 && console.IsInputRedirected)
        {
            await foreach (var line in console.Input.ReadAllLinesAsync())
                channelIds.Add(Snowflake.Parse(line));
        }

        if (channelIds.Count == 0)
        {
            throw new CommandException(
                "No channel IDs provided. "
                    + "Specify channel IDs as arguments or pipe them from the 'list channels' or 'list channels dm' commands."
            );
        }

        await console.Output.WriteLineAsync("Resolving channel(s)...");

        var channels = new List<Channel>();
        var channelsByGuild = new Dictionary<Snowflake, IReadOnlyList<Channel>>();

        foreach (var channelId in channelIds)
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

        await ExportAsync(console, channels);
    }
}

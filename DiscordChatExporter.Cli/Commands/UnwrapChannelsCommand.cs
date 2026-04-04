using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Cli.Utils.Extensions;
using DiscordChatExporter.Cli.Utils.Json;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Cli.Commands;

[Command(
    "list unwrap",
    Description = "Resolves categories and forums in a channel list to their child channels and threads."
)]
public partial class UnwrapChannelsCommand : DiscordCommandBase
{
    public override async ValueTask ExecuteAsync(IConsole console)
    {
        await base.ExecuteAsync(console);

        var cancellationToken = console.RegisterCancellationHandler();

        // Read all JSON from stdin (produced by 'list channels' or 'list channels dm')
        var sb = new StringBuilder();
        await foreach (var line in console.Input.ReadLinesAsync(cancellationToken))
            sb.Append(line);

        Channel[] channels;
        try
        {
            channels =
                JsonSerializer.Deserialize(
                    sb.ToString().Trim(),
                    CliJsonSerializerContext.Instance.ChannelArray
                ) ?? [];
        }
        catch (JsonException)
        {
            throw new CommandException(
                "Failed to parse input as a JSON channel array. "
                    + "Pipe the output of 'list channels' or 'list channels dm' to this command."
            );
        }

        var result = new List<Channel>();
        var channelsByGuild = new Dictionary<Snowflake, IReadOnlyList<Channel>>();

        foreach (var channel in channels)
        {
            if (channel.IsCategory)
            {
                // Expand category to its child channels
                var guildChannels =
                    channelsByGuild.GetValueOrDefault(channel.GuildId)
                    ?? await Discord.GetGuildChannelsAsync(channel.GuildId, cancellationToken);

                foreach (var guildChannel in guildChannels)
                {
                    if (guildChannel.Parent?.Id == channel.Id)
                        result.Add(guildChannel);
                }

                channelsByGuild[channel.GuildId] = guildChannels;
            }
            else if (channel.Kind == ChannelKind.GuildForum)
            {
                // Expand forum to its thread posts
                await foreach (
                    var thread in Discord.GetChannelThreadsAsync(
                        [channel],
                        cancellationToken: cancellationToken
                    )
                )
                    result.Add(thread);
            }
            else
            {
                result.Add(channel);
            }
        }

        await console.Output.WriteLineAsync(
            JsonSerializer.Serialize(
                result.ToArray(),
                CliJsonSerializerContext.Instance.ChannelArray
            )
        );
    }
}

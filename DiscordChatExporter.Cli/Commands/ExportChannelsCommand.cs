using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Cli.Utils.Extensions;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Cli.Commands;

[Command("export", Description = "Exports one or multiple channels.")]
public partial class ExportChannelsCommand : ExportCommandBase
{
    [CommandParameter(
        0,
        Name = "channel-ids",
        Description = "Channel ID(s). "
            + "If not provided, channel IDs are read from standard input (one per line or as a JSON array), "
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
            await foreach (var line in console.Input.ReadLinesAsync(cancellationToken))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;

                // JSON array produced by 'list channels' / 'list channels dm'
                if (trimmed.StartsWith('['))
                {
                    using var doc = JsonDocument.Parse(trimmed);
                    foreach (var element in doc.RootElement.EnumerateArray())
                        channelIds.Add(Snowflake.Parse(element.GetProperty("id").GetString()!));
                }
                else
                {
                    channelIds.Add(Snowflake.Parse(trimmed));
                }
            }
        }

        if (channelIds.Count == 0)
        {
            throw new CommandException(
                "No channel IDs provided. "
                    + "Specify channel IDs as arguments or pipe them from a newline-separated list."
            );
        }

        await console.Output.WriteLineAsync("Resolving channel(s)...");

        var channels = new List<Channel>();

        foreach (var channelId in channelIds)
        {
            var channel = await Discord.GetChannelAsync(channelId, cancellationToken);
            channels.Add(channel);
        }

        await ExportAsync(console, channels);
    }
}

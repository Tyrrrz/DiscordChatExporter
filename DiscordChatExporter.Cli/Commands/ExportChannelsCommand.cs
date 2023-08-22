using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Core.Discord;

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

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        await base.ExecuteAsync(console);
        await ExportAsync(console, ChannelIds);
    }
}

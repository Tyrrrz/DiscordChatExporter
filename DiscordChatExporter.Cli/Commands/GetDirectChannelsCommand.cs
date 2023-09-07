using System;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Cli.Commands;

[Command("dm", Description = "Gets the list of all direct message channels.")]
public class GetDirectChannelsCommand : DiscordCommandBase
{
    public override async ValueTask ExecuteAsync(IConsole console)
    {
        await base.ExecuteAsync(console);

        var cancellationToken = console.RegisterCancellationHandler();

        var channels = (
            await Discord.GetGuildChannelsAsync(Guild.DirectMessages.Id, cancellationToken)
        )
            .OrderByDescending(c => c.LastMessageId)
            .ThenBy(c => c.Name)
            .ToArray();

        var channelIdMaxLength = channels
            .Select(c => c.Id.ToString().Length)
            .OrderDescending()
            .FirstOrDefault();

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
        }
    }
}

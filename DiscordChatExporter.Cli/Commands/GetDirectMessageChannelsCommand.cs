using System;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Cli.Commands;

[Command("dm", Description = "Get the list of direct message channels.")]
public class GetDirectMessageChannelsCommand : TokenCommandBase
{
    public override async ValueTask ExecuteAsync(IConsole console)
    {
        var cancellationToken = console.RegisterCancellationHandler();

        var channels = await Discord.GetGuildChannelsAsync(Guild.DirectMessages.Id, cancellationToken);

        var textChannels = channels
            .Where(c => c.IsTextChannel)
            .OrderBy(c => c.Category.Position)
            .ThenBy(c => c.Name)
            .ToArray();

        foreach (var channel in textChannels)
        {
            // Channel ID
            await console.Output.WriteAsync(channel.Id.ToString());

            // Separator
            using (console.WithForegroundColor(ConsoleColor.DarkGray))
                await console.Output.WriteAsync(" | ");

            // Channel category / name
            using (console.WithForegroundColor(ConsoleColor.White))
                await console.Output.WriteLineAsync($"{channel.Category.Name} / {channel.Name}");
        }
    }
}
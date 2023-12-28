using System;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Cli.Commands;

[Command("guilds", Description = "Gets the list of accessible servers.")]
public class GetGuildsCommand : DiscordCommandBase
{
    public override async ValueTask ExecuteAsync(IConsole console)
    {
        await base.ExecuteAsync(console);

        var cancellationToken = console.RegisterCancellationHandler();

        var guilds = (await Discord.GetUserGuildsAsync(cancellationToken))
            // Show direct messages first
            .OrderByDescending(g => g.Id == Guild.DirectMessages.Id)
            .ThenBy(g => g.Name)
            .ToArray();

        var guildIdMaxLength = guilds
            .Select(g => g.Id.ToString().Length)
            .OrderDescending()
            .FirstOrDefault();

        foreach (var guild in guilds)
        {
            // Guild ID
            await console.Output.WriteAsync(guild.Id.ToString().PadRight(guildIdMaxLength, ' '));

            // Separator
            using (console.WithForegroundColor(ConsoleColor.DarkGray))
                await console.Output.WriteAsync(" | ");

            // Guild name
            using (console.WithForegroundColor(ConsoleColor.White))
                await console.Output.WriteLineAsync(guild.Name);
        }
    }
}

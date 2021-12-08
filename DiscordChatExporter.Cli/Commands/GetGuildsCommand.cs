using System;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Cli.Commands;

[Command("guilds", Description = "Get the list of accessible guilds.")]
public class GetGuildsCommand : TokenCommandBase
{
    public override async ValueTask ExecuteAsync(IConsole console)
    {
        var cancellationToken = console.RegisterCancellationHandler();

        var guilds = await Discord.GetUserGuildsAsync(cancellationToken);

        foreach (var guild in guilds.OrderBy(g => g.Name))
        {
            // Guild ID
            await console.Output.WriteAsync(guild.Id.ToString());

            // Separator
            using (console.WithForegroundColor(ConsoleColor.DarkGray))
                await console.Output.WriteAsync(" | ");

            // Guild name
            using (console.WithForegroundColor(ConsoleColor.White))
                await console.Output.WriteLineAsync(guild.Name);
        }
    }
}
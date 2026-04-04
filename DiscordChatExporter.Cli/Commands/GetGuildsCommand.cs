using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CliFx.Binding;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Cli.Utils.Json;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Cli.Commands;

[Command("list servers", Description = "Gets the list of accessible servers.")]
public partial class GetGuildsCommand : DiscordCommandBase
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

        await console.Output.WriteLineAsync(
            JsonSerializer.Serialize(guilds, CliJsonSerializerContext.Instance.GuildArray)
        );
    }
}

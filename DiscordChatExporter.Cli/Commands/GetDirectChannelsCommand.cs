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

[Command("list channels dm", Description = "Gets the list of direct message channels.")]
public partial class GetDirectChannelsCommand : DiscordCommandBase
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

        await console.Output.WriteLineAsync(
            JsonSerializer.Serialize(channels, CliJsonSerializerContext.Instance.ChannelArray)
        );
    }
}

using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Cli.Commands;

[Command("exportguild", Description = "Export all channels within specified guild.")]
public class ExportGuildCommand : ExportCommandBase
{
    [CommandOption("guild", 'g', IsRequired = true, Description = "Guild ID.")]
    public Snowflake GuildId { get; init; }

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        await base.ExecuteAsync(console);

        var cancellationToken = console.RegisterCancellationHandler();

        await console.Output.WriteLineAsync("Fetching channels...");
        var channels = await Discord.GetGuildChannelsAsync(GuildId, cancellationToken);
        var textChannels = channels.Where(c => c.IsTextChannel).ToArray();

        await base.ExecuteAsync(console, textChannels);
    }
}
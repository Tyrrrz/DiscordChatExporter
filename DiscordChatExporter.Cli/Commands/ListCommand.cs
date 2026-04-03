using System.Threading.Tasks;
using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;

namespace DiscordChatExporter.Cli.Commands;

[Command("list", Description = "Lists channels, DMs, or servers.")]
public partial class ListCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) =>
        throw new CommandException(
            "A subcommand is required ('channels', 'channels dm', or 'guilds').",
            showHelp: true
        );
}

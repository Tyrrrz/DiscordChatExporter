using System.Threading.Tasks;
using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;

namespace DiscordChatExporter.Cli.Commands;

[Command("list", Description = "Lists channels, DMs, or servers.")]
public partial class ListCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) =>
        throw new CommandException("Use one of the named commands listed below.", showHelp: true);
}

using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using DiscordChatExporter.Cli.Commands.Base;

namespace DiscordChatExporter.Cli.Commands
{
    [Command("export", Description = "Export a channel.")]
    public class ExportChannelCommand : ExportCommandBase
    {
        [CommandOption("channel", 'c', IsRequired = true,
            Description = "Channel ID.")]
        public string ChannelId { get; set; } = "";

        public override async ValueTask ExecuteAsync(IConsole console)
        {
            await base.ExecuteAsync(console);
            await ExportAsync(console, ChannelId);
        }
    }
}
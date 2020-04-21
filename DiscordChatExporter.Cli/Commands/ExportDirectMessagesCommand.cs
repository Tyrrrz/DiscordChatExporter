using System.Linq;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using DiscordChatExporter.Cli.Commands.Base;

namespace DiscordChatExporter.Cli.Commands
{
    [Command("exportdm", Description = "Export all direct message channels.")]
    public class ExportDirectMessagesCommand : ExportMultipleCommandBase
    {
        public override async ValueTask ExecuteAsync(IConsole console)
        {
            var directMessageChannels = await GetDiscordClient().GetDirectMessageChannelsAsync();
            var channels = directMessageChannels.OrderBy(c => c.Name).ToArray();

            await ExportMultipleAsync(console, channels);
        }
    }
}
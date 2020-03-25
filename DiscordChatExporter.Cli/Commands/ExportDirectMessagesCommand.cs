using System.Linq;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using DiscordChatExporter.Core.Services;

namespace DiscordChatExporter.Cli.Commands
{
    [Command("exportdm", Description = "Export all direct message channels.")]
    public class ExportDirectMessagesCommand : ExportMultipleCommandBase
    {
        public ExportDirectMessagesCommand(SettingsService settingsService, DataService dataService, ExportService exportService)
            : base(settingsService, dataService, exportService)
        {
        }

        public override async ValueTask ExecuteAsync(IConsole console)
        {
            var directMessageChannels = await DataService.GetDirectMessageChannelsAsync(Token);
            var channels = directMessageChannels.OrderBy(c => c.Name).ToArray();

            await ExportMultipleAsync(console, channels);
        }
    }
}
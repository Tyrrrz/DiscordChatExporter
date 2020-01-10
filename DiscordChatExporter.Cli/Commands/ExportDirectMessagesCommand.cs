using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;
using DiscordChatExporter.Core.Models.Exceptions;
using DiscordChatExporter.Core.Services;
using DiscordChatExporter.Core.Services.Exceptions;

namespace DiscordChatExporter.Cli.Commands
{
    [Command("exportdm", Description = "Export all direct message channels.")]
    public class ExportDirectMessagesCommand : ExportCommandBase
    {
        public ExportDirectMessagesCommand(SettingsService settingsService, DataService dataService, ExportService exportService)
            : base(settingsService, dataService, exportService)
        {
        }

        public override async Task ExecuteAsync(IConsole console)
        {
            // Get channels
            var channels = await DataService.GetDirectMessageChannelsAsync(GetToken());

            // Order channels
            channels = channels.OrderBy(c => c.Name).ToArray();

            // Loop through channels
            foreach (var channel in channels)
            {
                try
                {
                    await ExportAsync(console, channel);
                }
                catch (HttpErrorStatusCodeException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
                {
                    console.Error.WriteLine("You don't have access to this channel.");
                }
                catch (HttpErrorStatusCodeException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    console.Error.WriteLine("This channel doesn't exist.");
                }
                catch (DomainException ex)
                {
                    console.Error.WriteLine(ex.Message);
                }
            }
        }
    }
}
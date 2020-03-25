using System.Linq;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Services;

namespace DiscordChatExporter.Cli.Commands
{
    [Command("exportguild", Description = "Export all channels within specified guild.")]
    public class ExportGuildCommand : ExportMultipleCommandBase
    {
        [CommandOption("guild", 'g', IsRequired = true, Description = "Guild ID.")]
        public string GuildId { get; set; } = "";

        public ExportGuildCommand(SettingsService settingsService, DataService dataService, ExportService exportService)
            : base(settingsService, dataService, exportService)
        {
        }

        public override async ValueTask ExecuteAsync(IConsole console)
        {
            var guildChannels = await DataService.GetGuildChannelsAsync(Token, GuildId);

            var channels = guildChannels
                .Where(c => c.Type.IsExportable())
                .OrderBy(c => c.Name)
                .ToArray();

            await ExportMultipleAsync(console, channels);
        }
    }
}
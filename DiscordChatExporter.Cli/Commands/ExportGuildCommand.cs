using System.Linq;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using DiscordChatExporter.Cli.Commands.Base;

namespace DiscordChatExporter.Cli.Commands
{
    [Command("exportguild", Description = "Export all channels within specified guild.")]
    public class ExportGuildCommand : ExportMultipleCommandBase
    {
        [CommandOption("guild", 'g', IsRequired = true, Description = "Guild ID.")]
        public string GuildId { get; set; } = "";

        public override async ValueTask ExecuteAsync(IConsole console)
        {
            var guildChannels = await GetDiscordClient().GetGuildChannelsAsync(GuildId);

            var channels = guildChannels
                .Where(c => c.IsTextChannel)
                .OrderBy(c => c.Name)
                .ToArray();

            await ExportMultipleAsync(console, channels);
        }
    }
}
using System.Linq;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Domain.Utilities;

namespace DiscordChatExporter.Cli.Commands
{
    [Command("exportall", Description = "Export all direct messages and all channels within all guilds.")]
    public class ExportAllGuildsCommand : ExportMultipleCommandBase
    {
        public override async ValueTask ExecuteAsync(IConsole console)
        {
            [CommandOption("exclude-dm", 'e', Description = "If this flag is present, direct messages will not be exported.")]
            public bool ExcludeDMs { get; set; }

            if(!ExcludeDMs){
                var dmChannels = await GetDiscordClient().GetGuildChannelsAsync(Guild.DirectMessages.Id);
                await ExportMultipleAsync(console, dmChannels);
            }

            var guilds = await GetDiscordClient().GetUserGuildsAsync();
            foreach (var guild in guilds.OrderBy(g => g.Name))
            {
                var guildChannels = await GetDiscordClient().GetGuildChannelsAsync(guild.Id);
                await ExportMultipleAsync(console, guildChannels);
            }
        }
    }
}

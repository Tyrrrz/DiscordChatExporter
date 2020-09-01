using System.Linq;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Domain.Utilities;

namespace DiscordChatExporter.Cli.Commands
{
    [Command("exportallguilds", Description = "Export all channels within specified all guilds.")]
    public class ExportAllGuildsCommand : TokenCommandBase
    {
        public override async ValueTask ExecuteAsync(IConsole console)
        {
            var guilds = await GetDiscordClient().GetUserGuildsAsync();
            foreach (var guild in guilds.OrderBy(g => g.Name))
            {
                var guildChannels = await GetDiscordClient().GetGuildChannelsAsync(guild.Id);
                await ExportMultipleAsync(console, guildChannels);
            }
        }
    }
}

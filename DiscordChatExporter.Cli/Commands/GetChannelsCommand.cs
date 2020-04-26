using System.Linq;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Domain.Utilities;

namespace DiscordChatExporter.Cli.Commands
{
    [Command("channels", Description = "Get the list of channels in specified guild.")]
    public class GetChannelsCommand : TokenCommandBase
    {
        [CommandOption("guild", 'g', IsRequired = true, Description = "Guild ID.")]
        public string GuildId { get; set; } = "";

        public override async ValueTask ExecuteAsync(IConsole console)
        {
            var guildChannels = await GetDiscordClient().GetGuildChannelsAsync(GuildId);

            foreach (var channel in guildChannels.OrderBy(c => c.Category).ThenBy(c => c.Name))
                console.Output.WriteLine($"{channel.Id} | {channel.Category} / {channel.Name}");
        }
    }
}
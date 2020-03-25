using System.Linq;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Services;

namespace DiscordChatExporter.Cli.Commands
{
    [Command("channels", Description = "Get the list of channels in specified guild.")]
    public class GetChannelsCommand : TokenCommandBase
    {
        [CommandOption("guild", 'g', IsRequired = true, Description = "Guild ID.")]
        public string GuildId { get; set; } = "";

        public GetChannelsCommand(DataService dataService)
            : base(dataService)
        {
        }

        public override async ValueTask ExecuteAsync(IConsole console)
        {
            var guildChannels = await DataService.GetGuildChannelsAsync(Token, GuildId);

            var channels = guildChannels
                .Where(c => c.Type.IsExportable())
                .OrderBy(c => c.Name)
                .ToArray();

            foreach (var channel in channels)
                console.Output.WriteLine($"{channel.Id} | {channel.Name}");
        }
    }
}
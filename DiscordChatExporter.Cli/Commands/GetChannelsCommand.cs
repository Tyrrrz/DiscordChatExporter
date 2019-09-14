using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;
using DiscordChatExporter.Core.Models;
using DiscordChatExporter.Core.Services;

namespace DiscordChatExporter.Cli.Commands
{
    [Command("channels", Description = "Get the list of channels in specified guild.")]
    public class GetChannelsCommand : TokenCommandBase
    {
        [CommandOption("guild", 'g', IsRequired = true, Description = "Guild ID.")]
        public string GuildId { get; set; }

        public GetChannelsCommand(DataService dataService)
            : base(dataService)
        {
        }

        public override async Task ExecuteAsync(IConsole console)
        {
            // Get channels
            var channels = await DataService.GetGuildChannelsAsync(GetToken(), GuildId);

            // Filter and order channels
            channels = channels.Where(c => c.Type == ChannelType.GuildTextChat).OrderBy(c => c.Name).ToArray();

            // Print result
            foreach (var channel in channels)
                console.Output.WriteLine($"{channel.Id} | {channel.Name}");
        }
    }
}
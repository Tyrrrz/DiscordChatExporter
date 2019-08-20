using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;
using DiscordChatExporter.Core.Services;

namespace DiscordChatExporter.Cli.Commands
{
    [Command("dm", Description = "Get the list of direct message channels.")]
    public class GetDirectMessageChannelsCommand : TokenCommandBase
    {
        public GetDirectMessageChannelsCommand(DataService dataService)
            : base(dataService)
        {
        }

        public override async Task ExecuteAsync(IConsole console)
        {
            // Get channels
            var channels = await DataService.GetDirectMessageChannelsAsync(GetToken());

            // Order channels
            channels = channels.OrderBy(c => c.Name).ToArray();

            // Print result
            foreach (var channel in channels)
                console.Output.WriteLine($"{channel.Id} | {channel.Name}");
        }
    }
}
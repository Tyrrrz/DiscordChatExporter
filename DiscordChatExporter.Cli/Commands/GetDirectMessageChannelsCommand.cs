using System.Linq;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
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

        public override async ValueTask ExecuteAsync(IConsole console)
        {
            var directMessageChannels = await DataService.GetDirectMessageChannelsAsync(Token);
            var channels = directMessageChannels.OrderBy(c => c.Name).ToArray();

            foreach (var channel in channels)
                console.Output.WriteLine($"{channel.Id} | {channel.Name}");
        }
    }
}
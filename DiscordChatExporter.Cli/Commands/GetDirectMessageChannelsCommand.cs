using System.Linq;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using DiscordChatExporter.Cli.Commands.Base;

namespace DiscordChatExporter.Cli.Commands
{
    [Command("dm", Description = "Get the list of direct message channels.")]
    public class GetDirectMessageChannelsCommand : TokenCommandBase
    {
        public override async ValueTask ExecuteAsync(IConsole console)
        {
            var directMessageChannels = await GetDiscordClient().GetDirectMessageChannelsAsync();
            var channels = directMessageChannels.OrderBy(c => c.Name).ToArray();

            foreach (var channel in channels)
                console.Output.WriteLine($"{channel.Id} | {channel.Name}");
        }
    }
}
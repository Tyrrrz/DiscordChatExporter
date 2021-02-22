using System.Linq;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Cli.Commands
{
    [Command("dm", Description = "Get the list of direct message channels.")]
    public class GetDirectMessageChannelsCommand : TokenCommandBase
    {
        public override async ValueTask ExecuteAsync(IConsole console)
        {
            var channels = await Discord.GetGuildChannelsAsync(Guild.DirectMessages.Id);

            foreach (var channel in channels.OrderBy(c => c.Name))
            {
                await console.Output.WriteLineAsync(
                    $"{channel.Id} | {channel.Category} / {channel.Name}"
                );
            }
        }
    }
}
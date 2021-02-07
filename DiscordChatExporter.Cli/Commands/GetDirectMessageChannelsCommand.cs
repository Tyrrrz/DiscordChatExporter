using System.Linq;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Discord.Models.Common;
using DiscordChatExporter.Domain.Utilities;

namespace DiscordChatExporter.Cli.Commands
{
    [Command("dm", Description = "Get the list of direct message channels.")]
    public class GetDirectMessageChannelsCommand : TokenCommandBase
    {
        public override async ValueTask ExecuteAsync(IConsole console)
        {
            var channels = await GetDiscordClient().GetGuildChannelsAsync(Guild.DirectMessages.Id);

            foreach (var channel in channels.OrderBy(c => c.Name))
                console.Output.WriteLine($"{channel.Id} | {channel.Category} / {channel.Name}");
        }
    }
}
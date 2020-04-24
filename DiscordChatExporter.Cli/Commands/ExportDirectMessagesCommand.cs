using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Utilities;

namespace DiscordChatExporter.Cli.Commands
{
    [Command("exportdm", Description = "Export all direct message channels.")]
    public class ExportDirectMessagesCommand : ExportMultipleCommandBase
    {
        public override async ValueTask ExecuteAsync(IConsole console)
        {
            var dmChannels = await GetDiscordClient().GetGuildChannelsAsync(Guild.DirectMessages.Id);
            await ExportMultipleAsync(console, dmChannels);
        }
    }
}
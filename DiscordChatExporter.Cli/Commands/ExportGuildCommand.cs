using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Domain.Discord;
using DiscordChatExporter.Domain.Utilities;

namespace DiscordChatExporter.Cli.Commands
{
    [Command("exportguild", Description = "Export all channels within specified guild.")]
    public class ExportGuildCommand : ExportMultipleCommandBase
    {
        [CommandOption("guild", 'g', IsRequired = true, Description = "Guild ID.")]
        public Snowflake GuildId { get; init; }

        public override async ValueTask ExecuteAsync(IConsole console)
        {
            var channels = await GetDiscordClient().GetGuildChannelsAsync(GuildId);
            await ExportMultipleAsync(console, channels);
        }
    }
}
using System.Linq;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Cli.Commands
{
    [Command("channels", Description = "Get the list of channels in a guild.")]
    public class GetChannelsCommand : TokenCommandBase
    {
        [CommandOption("guild", 'g', IsRequired = true, Description = "Guild ID.")]
        public Snowflake GuildId { get; init; }

        public override async ValueTask ExecuteAsync(IConsole console)
        {
            var channels = await Discord.GetGuildChannelsAsync(GuildId);

            foreach (var channel in channels.OrderBy(c => c.Category.Position).ThenBy(c => c.Name))
            {
                await console.Output.WriteLineAsync(
                    $"{channel.Id} | {channel.Category} / {channel.Name}"
                );
            }
        }
    }
}
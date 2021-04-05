using System;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Core.Discord;
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
                // Channel ID
                await console.Output.WriteAsync(channel.Id.ToString());

                // Separator
                using (console.WithForegroundColor(ConsoleColor.DarkGray))
                    await console.Output.WriteAsync(" | ");

                // Channel category / name
                using (console.WithForegroundColor(ConsoleColor.White))
                    await console.Output.WriteAsync($"{channel.Category} / {channel.Name}");

                await console.Output.WriteLineAsync();
            }
        }
    }
}
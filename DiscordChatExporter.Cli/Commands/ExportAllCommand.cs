using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Domain.Discord.Models;

namespace DiscordChatExporter.Cli.Commands
{
    [Command("exportall", Description = "Export all accessible channels.")]
    public class ExportAllCommand : ExportMultipleCommandBase
    {
        [CommandOption("include-dm", Description = "Whether to also export direct message channels.")]
        public bool IncludeDirectMessages { get; set; } = true;

        public override async ValueTask ExecuteAsync(IConsole console)
        {
            var channels = new List<Channel>();

            // Aggregate channels from all guilds
            await foreach (var guild in GetDiscordClient().GetUserGuildsAsync())
            {
                // Skip DMs if instructed to
                if (!IncludeDirectMessages && guild.Id == Guild.DirectMessages.Id)
                    continue;

                await foreach (var channel in GetDiscordClient().GetGuildChannelsAsync(guild.Id))
                {
                    channels.Add(channel);
                }
            }

            await ExportMultipleAsync(console, channels);
        }
    }
}

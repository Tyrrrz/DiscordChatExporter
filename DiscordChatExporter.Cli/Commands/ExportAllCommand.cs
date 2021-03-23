using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Cli.Commands
{
    [Command("exportall", Description = "Export all accessible channels.")]
    public class ExportAllCommand : ExportMultipleCommandBase
    {
        [CommandOption("include-dm", Description = "Include direct message channels.")]
        public bool IncludeDirectMessages { get; init; } = true;

        public override async ValueTask ExecuteAsync(IConsole console)
        {
            await base.ExecuteAsync(console);

            // Get channel metadata
            await console.Output.WriteLineAsync("Fetching channels...");

            var channels = new List<Channel>();

            // Aggregate channels from all guilds
            await foreach (var guild in Discord.GetUserGuildsAsync())
            {
                // Skip DMs if instructed to
                if (!IncludeDirectMessages && guild.Id == Guild.DirectMessages.Id)
                    continue;

                await foreach (var channel in Discord.GetGuildChannelsAsync(guild.Id))
                {
                    channels.Add(channel);
                }
            }

            // Export
            await ExportChannelsAsync(console, channels);
        }
    }
}
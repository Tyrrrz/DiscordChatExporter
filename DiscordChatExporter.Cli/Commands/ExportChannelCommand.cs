using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Cli.Utils.Extensions;
using DiscordChatExporter.Core.Discord;

namespace DiscordChatExporter.Cli.Commands
{
    [Command("export", Description = "Export a channel.")]
    public class ExportChannelCommand : ExportCommandBase
    {
        [CommandOption("channel", 'c', IsRequired = true, Description = "Channel ID.")]
        public Snowflake ChannelId { get; init; }

        public override async ValueTask ExecuteAsync(IConsole console)
        {
            await base.ExecuteAsync(console);

            // Get channel metadata
            await console.Output.WriteLineAsync("Fetching channel...");
            var channel = await Discord.GetChannelAsync(ChannelId);
            var guild = await Discord.GetGuildAsync(channel.GuildId);

            // Export
            await console.Output.WriteLineAsync("Exporting...");
            await console.CreateProgressTicker().StartAsync(async progressContext =>
            {
                await ExportChannelAsync(guild, channel, progressContext);
            });
            await console.Output.WriteLineAsync("Done.");
        }
    }
}
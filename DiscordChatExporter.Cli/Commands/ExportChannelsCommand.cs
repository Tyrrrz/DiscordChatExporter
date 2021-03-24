using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Cli.Commands
{
    [Command("export", Description = "Export one or multiple channels.")]
    public class ExportChannelsCommand : ExportCommandBase
    {
        // TODO: change this to plural with a breaking change
        [CommandOption("channel", 'c', IsRequired = true, Description = "Channel ID(s).")]
        public IReadOnlyList<Snowflake> ChannelIds { get; init; } = Array.Empty<Snowflake>();

        public override async ValueTask ExecuteAsync(IConsole console)
        {
            await base.ExecuteAsync(console);

            // Get channel metadata
            await console.Output.WriteLineAsync("Fetching channel(s)...");

            var channels = new List<Channel>();

            foreach (var channelId in ChannelIds)
            {
                var channel = await Discord.GetChannelAsync(channelId);
                channels.Add(channel);
            }

            // Export
            await ExportAsync(console, channels);
        }
    }
}
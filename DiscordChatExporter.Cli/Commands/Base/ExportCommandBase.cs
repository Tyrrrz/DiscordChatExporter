using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Utilities;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Exporting;

namespace DiscordChatExporter.Cli.Commands.Base
{
    public abstract class ExportCommandBase : TokenCommandBase
    {
        [CommandOption("output", 'o', Description = "Output file or directory path.")]
        public string OutputPath { get; set; } = Directory.GetCurrentDirectory();

        [CommandOption("format", 'f', Description = "Output file format.")]
        public ExportFormat[] ExportFormats { get; set; } = new [] { ExportFormat.HtmlDark };

        [CommandOption("after", Description = "Limit to messages sent after this date.")]
        public DateTimeOffset? After { get; set; }

        [CommandOption("before", Description = "Limit to messages sent before this date.")]
        public DateTimeOffset? Before { get; set; }

        [CommandOption("partition", 'p', Description = "Split output into partitions limited to this number of messages.")]
        public int? PartitionLimit { get; set; }

        [CommandOption("media", Description = "Download referenced media content.")]
        public bool ShouldDownloadMedia { get; set; }

        [CommandOption("dateformat", Description = "Date format used in output.")]
        public string DateFormat { get; set; } = "dd-MMM-yy hh:mm tt";

        protected ChannelExporter GetChannelExporter() => new ChannelExporter(GetDiscordClient());

        protected async ValueTask ExportAsync(IConsole console, Guild guild, Channel channel)
        {
            console.Output.Write($"Exporting channel '{channel.Category} / {channel.Name}'... ");
            var progress = console.CreateProgressTicker();

            var request = new ExportRequest(
                guild,
                channel,
                OutputPath,
                ExportFormats,
                After,
                Before,
                PartitionLimit,
                ShouldDownloadMedia,
                DateFormat
            );

            await GetChannelExporter().ExportChannelAsync(request, progress);

            console.Output.WriteLine();
            console.Output.WriteLine("Done.");
        }

        protected async ValueTask ExportAsync(IConsole console, Channel channel)
        {
            var guild = await GetDiscordClient().GetGuildAsync(channel.GuildId);
            await ExportAsync(console, guild, channel);
        }

        protected async ValueTask ExportAsync(IConsole console, string channelId)
        {
            var channel = await GetDiscordClient().GetChannelAsync(channelId);
            await ExportAsync(console, channel);
        }
    }
}
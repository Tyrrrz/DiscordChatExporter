using System.IO;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Utilities;
using DiscordChatExporter.Domain.Discord;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Exporting;

namespace DiscordChatExporter.Cli.Commands.Base
{
    public abstract class ExportCommandBase : TokenCommandBase
    {
        [CommandOption("output", 'o', Description = "Output file or directory path.")]
        public string OutputPath { get; init; } = Directory.GetCurrentDirectory();

        [CommandOption("format", 'f', Description = "Export format.")]
        public ExportFormat ExportFormat { get; init; } = ExportFormat.HtmlDark;

        [CommandOption("after", Description = "Only include messages sent after this date or message ID.")]
        public Snowflake? After { get; init; }

        [CommandOption("before", Description = "Only include messages sent before this date or message ID.")]
        public Snowflake? Before { get; init; }

        [CommandOption("partition", 'p', Description = "Split output into partitions limited to this number of messages.")]
        public int? PartitionLimit { get; init; }

        [CommandOption("media", Description = "Download referenced media content.")]
        public bool ShouldDownloadMedia { get; init; }

        [CommandOption("reuse-media", Description = "Reuse already existing media content to skip redundant downloads.")]
        public bool ShouldReuseMedia { get; init; }

        [CommandOption("dateformat", Description = "Format used when writing dates.")]
        public string DateFormat { get; init; } = "dd-MMM-yy hh:mm tt";

        protected ChannelExporter GetChannelExporter() => new(GetDiscordClient());

        protected async ValueTask ExportAsync(IConsole console, Guild guild, Channel channel)
        {
            console.Output.Write($"Exporting channel '{channel.Category} / {channel.Name}'... ");
            var progress = console.CreateProgressTicker();

            var request = new ExportRequest(
                guild,
                channel,
                OutputPath,
                ExportFormat,
                After,
                Before,
                PartitionLimit,
                ShouldDownloadMedia,
                ShouldReuseMedia,
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

        protected async ValueTask ExportAsync(IConsole console, Snowflake channelId)
        {
            var channel = await GetDiscordClient().GetChannelAsync(channelId);
            await ExportAsync(console, channel);
        }

        public override ValueTask ExecuteAsync(IConsole console)
        {
            if (ShouldReuseMedia && !ShouldDownloadMedia)
            {
                throw new CommandException("The --reuse-media option cannot be used without the --media option.");
            }

            return default;
        }
    }
}
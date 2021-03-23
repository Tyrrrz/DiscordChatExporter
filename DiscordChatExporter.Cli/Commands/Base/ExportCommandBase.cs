using System.IO;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exporting;
using Spectre.Console;

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

        private ChannelExporter? _channelExporter;
        protected ChannelExporter Exporter => _channelExporter ??= new ChannelExporter(Discord);

        protected async ValueTask ExportChannelAsync(Guild guild, Channel channel, ProgressContext progressContext)
        {
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

            var progress = progressContext.AddTask(
                $"{channel.Category} / {channel.Name}",
                new ProgressTaskSettings {MaxValue = 1}
            );

            try
            {
                await Exporter.ExportChannelAsync(request, progress);
            }
            finally
            {
                progress.StopTask();
            }
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
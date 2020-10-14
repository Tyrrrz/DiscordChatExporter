using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Internal;
using DiscordChatExporter.Domain.Internal.Extensions;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Domain.Exporting
{
    public partial class ExportRequest
    {
        public Guild Guild { get; }

        public Channel Channel { get; }

        public IEnumerable<ExportFormat> Formats { get; }

        public DateTimeOffset? After { get; }

        public DateTimeOffset? Before { get; }

        public int? PartitionLimit { get; }

        public bool ShouldDownloadMedia { get; }

        public string DateFormat { get; }

        public string OutputMediaDirPath { get; }

        public string OutputBaseDirPath { get; }

        private string OutputFilenameWithoutExtension { get; }

        public ExportRequest(
            Guild guild,
            Channel channel,
            string outputPath,
            IEnumerable<ExportFormat> formats,
            DateTimeOffset? after,
            DateTimeOffset? before,
            int? partitionLimit,
            bool shouldDownloadMedia,
            string dateFormat)
        {
            Guild = guild;
            Channel = channel;
            Formats = formats;
            After = after;
            Before = before;
            PartitionLimit = partitionLimit;
            ShouldDownloadMedia = shouldDownloadMedia;
            DateFormat = dateFormat;

            if (formats.IsNullOrEmpty())
            {
                throw new ArgumentException("At least one export format must be provided.");
            }

            var outputPathing = formats.Count() > 1 ? OutputPathing.ParseForMultipleOutputFormats(outputPath) :
                OutputPathing.ParseForSingleOutputFormat(outputPath);

            OutputBaseDirPath = outputPathing.DirectoryPath;
            OutputFilenameWithoutExtension = outputPathing.FilenameWithoutExt ?? GetDefaultOutputFileNameWithoutExtension(guild, channel, after, before);

            var mediaDirNameBase = formats.Count() > 1 ? OutputFilenameWithoutExtension : GetDefaultOutputFileName(guild, channel, formats.First(), after, before);
            OutputMediaDirPath = Path.Join(OutputBaseDirPath, mediaDirNameBase + "_Files") + Path.DirectorySeparatorChar;
        }

        public string GetOutputFilePathForFormat(ExportFormat format)
        {
            if (!Formats.Includes(format))
            {
                throw new ArgumentException($"Format {format} was not included in this export request.");
            }

            var dir = OutputBaseDirPath;
            var file = $"{OutputFilenameWithoutExtension}.{format.GetFileExtension()}" ?? GetDefaultOutputFileName(Guild, Channel, format, After, Before);

            return Path.Join(dir, file);
        }
    }

    public partial class ExportRequest
    {
        public static string GetDefaultOutputFileName(
            Guild guild,
            Channel channel,
            ExportFormat format,
            DateTimeOffset? after = null,
            DateTimeOffset? before = null)
        {
            var withoutExtension = GetDefaultOutputFileNameWithoutExtension(guild, channel, after, before);
            return $"{withoutExtension}.{format.GetFileExtension()}";
        }

        private static string GetDefaultOutputFileNameWithoutExtension(
            Guild guild,
            Channel channel,
            DateTimeOffset? after = null,
            DateTimeOffset? before = null)
        {

            var buffer = new StringBuilder();

            // Guild and channel names
            buffer.Append($"{guild.Name} - {channel.Category} - {channel.Name} [{channel.Id}]");

            // Date range
            if (after != null || before != null)
            {
                buffer.Append(" (");

                // Both 'after' and 'before' are set
                if (after != null && before != null)
                {
                    buffer.Append($"{after:yyyy-MM-dd} to {before:yyyy-MM-dd}");
                }
                // Only 'after' is set
                else if (after != null)
                {
                    buffer.Append($"after {after:yyyy-MM-dd}");
                }
                // Only 'before' is set
                else
                {
                    buffer.Append($"before {before:yyyy-MM-dd}");
                }

                buffer.Append(")");
            }

            // Replace invalid chars
            PathEx.EscapePath(buffer);

            return buffer.ToString();
        }
    }
}
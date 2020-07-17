using System;
using System.IO;
using System.Text;
using DiscordChatExporter.Domain.Discord.Models;

namespace DiscordChatExporter.Domain.Exporting
{
    public partial class ExportRequest
    {
        public Guild Guild { get; }

        public Channel Channel { get; }

        public string OutputPath { get; }

        public string OutputBaseFilePath { get; }

        public string OutputBaseDirPath { get; }

        public string OutputMediaDirPath { get; }

        public ExportFormat Format { get; }

        public DateTimeOffset? After { get; }

        public DateTimeOffset? Before { get; }

        public int? PartitionLimit { get; }

        public bool ShouldDownloadMedia { get; }

        public string DateFormat { get; }

        public ExportRequest(Guild guild,
            Channel channel,
            string outputPath,
            ExportFormat format,
            DateTimeOffset? after,
            DateTimeOffset? before,
            int? partitionLimit,
            bool shouldDownloadMedia,
            string dateFormat)
        {
            Guild = guild;
            Channel = channel;
            OutputPath = outputPath;
            Format = format;
            After = after;
            Before = before;
            PartitionLimit = partitionLimit;
            ShouldDownloadMedia = shouldDownloadMedia;
            DateFormat = dateFormat;

            OutputBaseFilePath = GetOutputBaseFilePath(
                guild,
                channel,
                outputPath,
                format,
                after,
                before
            );

            OutputBaseDirPath = Path.GetDirectoryName(OutputBaseFilePath) ?? outputPath;
            OutputMediaDirPath = $"{OutputBaseFilePath}_Files{Path.PathSeparator}";
        }
    }

    public partial class ExportRequest
    {
        private static string GetOutputBaseFilePath(
            Guild guild,
            Channel channel,
            string outputPath,
            ExportFormat format,
            DateTimeOffset? after = null,
            DateTimeOffset? before = null)
        {
            // Output is a directory
            if (Directory.Exists(outputPath) || string.IsNullOrWhiteSpace(Path.GetExtension(outputPath)))
            {
                var fileName = GetDefaultOutputFileName(guild, channel, format, after, before);
                return Path.Combine(outputPath, fileName);
            }

            // Output is a file
            return outputPath;
        }

        public static string GetDefaultOutputFileName(
            Guild guild,
            Channel channel,
            ExportFormat format,
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

            // File extension
            buffer.Append($".{format.GetFileExtension()}");

            // Replace invalid chars
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
                buffer.Replace(invalidChar, '_');

            return buffer.ToString();
        }
    }
}
using System.IO;
using System.Text;
using DiscordChatExporter.Domain.Discord;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Internal;

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

        public Snowflake? After { get; }

        public Snowflake? Before { get; }

        public int? PartitionLimit { get; }

        public bool ShouldDownloadMedia { get; }

        public bool ShouldReuseMedia { get; }

        public string DateFormat { get; }

        public ExportRequest(
            Guild guild,
            Channel channel,
            string outputPath,
            ExportFormat format,
            Snowflake? after,
            Snowflake? before,
            int? partitionLimit,
            bool shouldDownloadMedia,
            bool shouldReuseMedia,
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
            ShouldReuseMedia = shouldReuseMedia;
            DateFormat = dateFormat;

            OutputBaseFilePath = GetOutputBaseFilePath(
                guild,
                channel,
                outputPath,
                format,
                after,
                before
            );

            OutputBaseDirPath = Path.GetDirectoryName(OutputBaseFilePath) ?? OutputPath;
            OutputMediaDirPath = $"{OutputBaseFilePath}_Files{Path.DirectorySeparatorChar}";
        }
    }

    public partial class ExportRequest
    {
        private static string GetOutputBaseFilePath(
            Guild guild,
            Channel channel,
            string outputPath,
            ExportFormat format,
            Snowflake? after = null,
            Snowflake? before = null)
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
            Snowflake? after = null,
            Snowflake? before = null)
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
                    buffer.Append($"{after?.ToDate():yyyy-MM-dd} to {before?.ToDate():yyyy-MM-dd}");
                }
                // Only 'after' is set
                else if (after != null)
                {
                    buffer.Append($"after {after?.ToDate():yyyy-MM-dd}");
                }
                // Only 'before' is set
                else
                {
                    buffer.Append($"before {before?.ToDate():yyyy-MM-dd}");
                }

                buffer.Append(")");
            }

            // File extension
            buffer.Append($".{format.GetFileExtension()}");

            // Replace invalid chars
            PathEx.EscapePath(buffer);

            return buffer.ToString();
        }
    }
}
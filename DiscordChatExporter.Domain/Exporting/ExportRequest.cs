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

        public ExportFormat Format { get; }

        public string DateFormat { get; }

        public DateTimeOffset? After { get; }

        public DateTimeOffset? Before { get; }

        public int? PartitionLimit { get; }

        public bool RewriteMedia { get; }

        public ExportRequest(
            Guild guild,
            Channel channel,
            string outputPath,
            ExportFormat format,
            string dateFormat,
            DateTimeOffset? after,
            DateTimeOffset? before,
            int? partitionLimit,
            bool rewriteMedia)
        {
            Guild = guild;
            Channel = channel;
            OutputPath = outputPath;
            Format = format;
            DateFormat = dateFormat;
            After = after;
            Before = before;
            PartitionLimit = partitionLimit;
            RewriteMedia = rewriteMedia;
        }

        public string GetOutputBaseFilePath()
        {
            // Output is a directory
            if (Directory.Exists(OutputPath) || string.IsNullOrWhiteSpace(Path.GetExtension(OutputPath)))
            {
                var fileName = GetDefaultOutputFileName(Guild, Channel, Format, After, Before);
                return Path.Combine(OutputPath, fileName);
            }

            // Output is a file
            return OutputPath;
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
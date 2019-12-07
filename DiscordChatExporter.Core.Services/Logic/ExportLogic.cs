using System;
using System.IO;
using System.Text;
using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Core.Services.Logic
{
    public static class ExportLogic
    {
        public static string GetDefaultExportFileName(ExportFormat format,
            Guild guild, Channel channel,
            DateTimeOffset? after = null, DateTimeOffset? before = null)
        {
            var buffer = new StringBuilder();

            // Append guild and channel names
            buffer.Append($"{guild.Name} - {channel.Name} [{channel.Id}]");

            // Append date range
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

            // Append extension
            buffer.Append($".{format.GetFileExtension()}");

            // Replace invalid chars
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
                buffer.Replace(invalidChar, '_');

            return buffer.ToString();
        }

        public static string GetExportPartitionFilePath(string baseFilePath, int partitionIndex)
        {
            // First partition - no changes
            if (partitionIndex <= 0)
                return baseFilePath;

            // Inject partition index into file name
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(baseFilePath);
            var fileExt = Path.GetExtension(baseFilePath);
            var fileName = $"{fileNameWithoutExt} [part {partitionIndex + 1}]{fileExt}";

            // Generate new path
            var dirPath = Path.GetDirectoryName(baseFilePath);
            if (!string.IsNullOrWhiteSpace(dirPath))
                return Path.Combine(dirPath, fileName);

            return fileName;
        }
    }
}
using System;
using System.IO;
using System.Linq;
using System.Text;
using DiscordChatExporter.Core.Models;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Services.Helpers
{
    public static class ExportHelper
    {
        public static bool IsDirectoryPath(string path) =>
            path.Last() == Path.DirectorySeparatorChar ||
            path.Last() == Path.AltDirectorySeparatorChar ||
            (Path.GetExtension(path).IsNullOrWhiteSpace() && !File.Exists(path));

        public static string GetDefaultExportFileName(ExportFormat format, Guild guild, Channel channel,
            DateTimeOffset? after = null, DateTimeOffset? before = null)
        {
            var result = new StringBuilder();

            // Append guild and channel names
            result.Append($"{guild.Name} - {channel.Name} [{channel.Id}]");

            // Append date range
            if (after != null || before != null)
            {
                result.Append(" (");

                // Both 'after' and 'before' are set
                if (after != null && before != null)
                {
                    result.Append($"{after:yyyy-MM-dd} to {before:yyyy-MM-dd}");
                }
                // Only 'after' is set
                else if (after != null)
                {
                    result.Append($"after {after:yyyy-MM-dd}");
                }
                // Only 'before' is set
                else
                {
                    result.Append($"before {before:yyyy-MM-dd}");
                }

                result.Append(")");
            }

            // Append extension
            result.Append($".{format.GetFileExtension()}");

            // Replace invalid chars
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
                result.Replace(invalidChar, '_');

            return result.ToString();
        }
    }
}
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
    }
}
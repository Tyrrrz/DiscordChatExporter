using System;
using System.IO;
using System.Text;
using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Core.Helpers
{
    public static class ExportHelper
    {
        public static string GetDefaultExportFileName(ExportFormat format, Guild guild, Channel channel,
            DateTime? from = null, DateTime? to = null)
        {
            var result = new StringBuilder();

            // Append guild and channel names
            result.Append($"{guild.Name} - {channel.Name}");

            // Append date range
            if (from != null || to != null)
            {
                result.Append(" (");

                // Both 'from' and 'to' are set
                if (from != null && to != null)
                {
                    result.Append($"{from:yyyy-MM-dd} to {to:yyyy-MM-dd}");
                }
                // Only 'from' is set
                else if (from != null)
                {
                    result.Append($"after {from:yyyy-MM-dd}");
                }
                // Only 'to' is set
                else
                {
                    result.Append($"before {to:yyyy-MM-dd}");
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
using System;

namespace DiscordChatExporter.Core.Models
{
    public static class Extensions
    {
        public static string GetFileExtension(this ExportFormat format)
        {
            if (format == ExportFormat.PlainText)
                return "txt";
            if (format == ExportFormat.HtmlDark)
                return "html";
            if (format == ExportFormat.HtmlLight)
                return "html";

            throw new ArgumentOutOfRangeException(nameof(format));
        }

        public static string GetDisplayName(this ExportFormat format)
        {
            if (format == ExportFormat.PlainText)
                return "Plain Text";
            if (format == ExportFormat.HtmlDark)
                return "HTML (Dark)";
            if (format == ExportFormat.HtmlLight)
                return "HTML (Light)";

            throw new ArgumentOutOfRangeException(nameof(format));
        }
    }
}
using System;

namespace DiscordChatExporter.Models
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

            throw new NotImplementedException();
        }

        public static string GetDisplayName(this ExportFormat format)
        {
            if (format == ExportFormat.PlainText)
                return "Plain Text";
            if (format == ExportFormat.HtmlDark)
                return "HTML (Dark)";
            if (format == ExportFormat.HtmlLight)
                return "HTML (Light)";

            throw new NotImplementedException();
        }
    }
}
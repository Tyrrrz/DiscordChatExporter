using System;

namespace DiscordChatExporter.Core.Models
{
    public static class Extensions
    {
        public static string GetFileExtension(this ExportFormat format) =>
            format switch
            {
                ExportFormat.PlainText => "txt",
                ExportFormat.HtmlDark => "html",
                ExportFormat.HtmlLight => "html",
                ExportFormat.Csv => "csv",
                _ => throw new ArgumentOutOfRangeException(nameof(format))
            };

        public static string GetDisplayName(this ExportFormat format) =>
            format switch
            {
                ExportFormat.PlainText => "Plain Text",
                ExportFormat.HtmlDark => "HTML (Dark)",
                ExportFormat.HtmlLight => "HTML (Light)",
                ExportFormat.Csv => "Comma Seperated Values (CSV)",
                _ => throw new ArgumentOutOfRangeException(nameof(format))
            };
    }
}
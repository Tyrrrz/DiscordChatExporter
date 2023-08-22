using System;

namespace DiscordChatExporter.Core.Exporting;

public enum ExportFormat
{
    PlainText,
    HtmlDark,
    HtmlLight,
    Csv,
    Json
}

public static class ExportFormatExtensions
{
    public static string GetFileExtension(this ExportFormat format) =>
        format switch
        {
            ExportFormat.PlainText => "txt",
            ExportFormat.HtmlDark => "html",
            ExportFormat.HtmlLight => "html",
            ExportFormat.Csv => "csv",
            ExportFormat.Json => "json",
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };

    public static string GetDisplayName(this ExportFormat format) =>
        format switch
        {
            ExportFormat.PlainText => "TXT",
            ExportFormat.HtmlDark => "HTML (Dark)",
            ExportFormat.HtmlLight => "HTML (Light)",
            ExportFormat.Csv => "CSV",
            ExportFormat.Json => "JSON",
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
}

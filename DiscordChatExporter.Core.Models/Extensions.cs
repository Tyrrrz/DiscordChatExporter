using System;

namespace DiscordChatExporter.Core.Models
{
    public static class Extensions
    {
        public static bool IsExportable(this ChannelType channelType) =>
            channelType == ChannelType.GuildTextChat ||
            channelType == ChannelType.DirectTextChat ||
            channelType == ChannelType.DirectGroupTextChat ||
            channelType == ChannelType.GuildNews ||
            channelType == ChannelType.GuildStore;

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
                ExportFormat.PlainText => "Plain Text",
                ExportFormat.HtmlDark => "HTML (Dark)",
                ExportFormat.HtmlLight => "HTML (Light)",
                ExportFormat.Csv => "Comma Separated Values (CSV)",
                ExportFormat.Json => "JavaScript Object Notation (JSON)",
                _ => throw new ArgumentOutOfRangeException(nameof(format))
            };
    }
}
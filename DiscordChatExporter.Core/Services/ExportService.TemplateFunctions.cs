using System;
using System.Text.RegularExpressions;
using DiscordChatExporter.Core.Internal;
using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Core.Services
{
    public partial class ExportService
    {
        private string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString(_settingsService.DateFormat);
        }

        private string FormatFileSize(long fileSize)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            double size = fileSize;
            var unit = 0;

            while (size >= 1024)
            {
                size /= 1024;
                ++unit;
            }

            return $"{size:0.#} {units[unit]}";
        }

        private string FormatMessageContentPlainText(Message message)
        {
            var content = message.Content;

            // New lines
            content = content.Replace("\n", Environment.NewLine);

            // User mentions (<@id> and <@!id>)
            foreach (var mentionedUser in message.Mentions.Users)
                content = Regex.Replace(content, $"<@!?{mentionedUser.Id}>", $"@{mentionedUser}");

            // Channel mentions (<#id>)
            foreach (var mentionedChannel in message.Mentions.Channels)
                content = content.Replace($"<#{mentionedChannel.Id}>", $"#{mentionedChannel.Name}");

            // Role mentions (<@&id>)
            foreach (var mentionedRole in message.Mentions.Roles)
                content = content.Replace($"<@&{mentionedRole.Id}>", $"@{mentionedRole.Name}");

            // Custom emojis (<:name:id>)
            content = Regex.Replace(content, "<(:.*?:)\\d*>", "$1");

            return content;
        }

        private string FormatMessageContentHtml(Message message)
        {
            var content = message.Content;

            // Convert content markdown to HTML
            content = MarkdownProcessor.ToHtml(content);

            // Meta mentions (@everyone)
            content = content.Replace("@everyone", "<span class=\"mention\">@everyone</span>");

            // Meta mentions (@here)
            content = content.Replace("@here", "<span class=\"mention\">@here</span>");

            // User mentions (<@id> and <@!id>)
            foreach (var mentionedUser in message.Mentions.Users)
            {
                content = Regex.Replace(content, $"&lt;@!?{mentionedUser.Id}&gt;",
                    $"<span class=\"mention\" title=\"{mentionedUser.FullName.HtmlEncode()}\">" +
                    $"@{mentionedUser.Name.HtmlEncode()}" +
                    "</span>");
            }

            // Role mentions (<@&id>)
            foreach (var mentionedRole in message.Mentions.Roles)
            {
                content = content.Replace($"&lt;@&amp;{mentionedRole.Id}&gt;",
                    "<span class=\"mention\">" +
                    $"@{mentionedRole.Name.HtmlEncode()}" +
                    "</span>");
            }

            // Channel mentions (<#id>)
            foreach (var mentionedChannel in message.Mentions.Channels)
            {
                content = content.Replace($"&lt;#{mentionedChannel.Id}&gt;",
                    "<span class=\"mention\">" +
                    $"#{mentionedChannel.Name.HtmlEncode()}" +
                    "</span>");
            }

            // Custom emojis (<:name:id>)
            content = Regex.Replace(content, "&lt;(:.*?:)(\\d*)&gt;",
                "<img class=\"emoji\" title=\"$1\" src=\"https://cdn.discordapp.com/emojis/$2.png\" />");

            return content;
        }

        private string FormatMessageContentCsv(Message message)
        {
            var content = message.Content;

            // New lines
            content = content.Replace("\n", ", ");

            // Escape quotes
            content = content.Replace("\"", "\"\"");

            // Escape commas and semicolons
            if (content.Contains(",") || content.Contains(";"))
                content = $"\"{content}\"";

            // User mentions (<@id> and <@!id>)
            foreach (var mentionedUser in message.Mentions.Users)
                content = Regex.Replace(content, $"<@!?{mentionedUser.Id}>", $"@{mentionedUser}");

            // Channel mentions (<#id>)
            foreach (var mentionedChannel in message.Mentions.Channels)
                content = content.Replace($"<#{mentionedChannel.Id}>", $"#{mentionedChannel.Name}");

            // Role mentions (<@&id>)
            foreach (var mentionedRole in message.Mentions.Roles)
                content = content.Replace($"<@&{mentionedRole.Id}>", $"@{mentionedRole.Name}");

            // Custom emojis (<:name:id>)
            content = Regex.Replace(content, "<(:.*?:)\\d*>", "$1");

            return content;
        }

        private string FormatMessageContent(ExportFormat format, Message message)
        {
            if (format == ExportFormat.PlainText)
                return FormatMessageContentPlainText(message);

            if (format == ExportFormat.HtmlDark)
                return FormatMessageContentHtml(message);

            if (format == ExportFormat.HtmlLight)
                return FormatMessageContentHtml(message);

            if (format == ExportFormat.Csv)
                return FormatMessageContentCsv(message);

            throw new ArgumentOutOfRangeException(nameof(format));
        }
    }
}
using System;
using System.Drawing;
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

        private string FormatColor(Color color)
        {
            return $"{color.R},{color.G},{color.B},{color.A}";
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

        private string FormatContentHtml(string content, bool allowLinks = false, MentionContainer mentions = null)
        {
            // Convert content markdown to HTML
            content = MarkdownProcessor.ToHtml(content, allowLinks);

            // Meta mentions (@everyone)
            content = content.Replace("@everyone", "<span class=\"mention\">@everyone</span>");

            // Meta mentions (@here)
            content = content.Replace("@here", "<span class=\"mention\">@here</span>");

            if (mentions != null)
            {
                // User mentions (<@id> and <@!id>)
                foreach (var mentionedUser in mentions.Users)
                {
                    content = Regex.Replace(content, $"&lt;@!?{mentionedUser.Id}&gt;",
                        $"<span class=\"mention\" title=\"{mentionedUser.FullName.HtmlEncode()}\">" +
                        $"@{mentionedUser.Name.HtmlEncode()}" +
                        "</span>");
                }

                // Role mentions (<@&id>)
                foreach (var mentionedRole in mentions.Roles)
                {
                    content = content.Replace($"&lt;@&amp;{mentionedRole.Id}&gt;",
                        "<span class=\"mention\">" +
                        $"@{mentionedRole.Name.HtmlEncode()}" +
                        "</span>");
                }

                // Channel mentions (<#id>)
                foreach (var mentionedChannel in mentions.Channels)
                {
                    content = content.Replace($"&lt;#{mentionedChannel.Id}&gt;",
                        "<span class=\"mention\">" +
                        $"#{mentionedChannel.Name.HtmlEncode()}" +
                        "</span>");
                }
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
    }
}
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DiscordChatExporter.Core.Internal;
using DiscordChatExporter.Core.Models;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Services
{
    public partial class ExportService
    {
        private static class TemplateFunctions
        {
            public static string HtmlEncode(string str)
            {
                return str.HtmlEncode();
            }

            public static string HtmlDecode(string str)
            {
                return str.HtmlDecode();
            }

            public static string Format(IFormattable obj, string format)
            {
                return obj.ToString(format, CultureInfo.InvariantCulture);
            }

            public static string FormatFileSize(long fileSize)
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

            public static string FormatColor(Color color)
            {
                return $"{color.R},{color.G},{color.B},{color.A}";
            }

            public static string FormatMessageContentPlainText(Message message)
            {
                var content = message.Content;

                // New lines
                content = content.Replace("\n", Environment.NewLine);

                //// User mentions (<@id> and <@!id>)
                //foreach (var mentionedUser in message.Mentions.Users)
                //    content = Regex.Replace(content, $"<@!?{mentionedUser.Id}>", $"@{mentionedUser}");

                //// Channel mentions (<#id>)
                //foreach (var mentionedChannel in message.Mentions.Channels)
                //    content = content.Replace($"<#{mentionedChannel.Id}>", $"#{mentionedChannel.Name}");

                //// Role mentions (<@&id>)
                //foreach (var mentionedRole in message.Mentions.Roles)
                //    content = content.Replace($"<@&{mentionedRole.Id}>", $"@{mentionedRole.Name}");

                // Custom emojis (<:name:id>)
                content = Regex.Replace(content, "<(:.*?:)\\d*>", "$1");

                return content;
            }

            public static string FormatContentHtml(string content, bool allowLinks = false, GuildMentionables mentions = null)
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
                    var mentionedUserIds = Regex.Matches(content, "&lt;@!?(\\d+)&gt;")
                        .Cast<Match>()
                        .Select(m => m.Groups[1].Value)
                        .ExceptBlank()
                        .ToArray();

                    foreach (var mentionedUserId in mentionedUserIds)
                    {
                        var mentionedUser = mentions.GetUser(mentionedUserId);
                        content = Regex.Replace(content, $"&lt;@!?{mentionedUserId}&gt;",
                            $"<span class=\"mention\" title=\"{mentionedUser.FullName.HtmlEncode()}\">" +
                            $"@{mentionedUser.Name.HtmlEncode()}" +
                            "</span>");
                    }

                    // Channel mentions (<#id>)
                    var mentionedChannelIds = Regex.Matches(content, "&lt;#(\\d+)&gt;")
                        .Cast<Match>()
                        .Select(m => m.Groups[1].Value)
                        .ExceptBlank()
                        .ToArray();

                    foreach (var mentionedChannelId in mentionedChannelIds)
                    {
                        var mentionedChannel = mentions.GetChannel(mentionedChannelId);
                        content = content.Replace($"&lt;#{mentionedChannelId}&gt;",
                            "<span class=\"mention\">" +
                            $"#{mentionedChannel.Name.HtmlEncode()}" +
                            "</span>");
                    }

                    // Role mentions (<@&id>)
                    var mentionedRoleIds = Regex.Matches(content, "&lt;@&amp;(\\d+)&gt;")
                        .Cast<Match>()
                        .Select(m => m.Groups[1].Value)
                        .ExceptBlank()
                        .ToArray();

                    foreach (var mentionedRoleId in mentionedRoleIds)
                    {
                        var mentionedRole = mentions.GetRole(mentionedRoleId);
                        content = content.Replace($"&lt;@&amp;{mentionedRoleId}&gt;",
                            "<span class=\"mention\">" +
                            $"@{mentionedRole.Name.HtmlEncode()}" +
                            "</span>");
                    }
                }

                // Custom emojis (<:name:id>)
                content = Regex.Replace(content, "&lt;(:.*?:)(\\d*)&gt;",
                    "<img class=\"emoji\" title=\"$1\" src=\"https://cdn.discordapp.com/emojis/$2.png\" />");

                return content;
            }

            public static string FormatMessageContentCsv(Message message)
            {
                var content = message.Content;

                // New lines
                content = content.Replace("\n", ", ");

                // Escape quotes
                content = content.Replace("\"", "\"\"");

                // Escape commas and semicolons
                if (content.Contains(",") || content.Contains(";"))
                    content = $"\"{content}\"";

                //// User mentions (<@id> and <@!id>)
                //foreach (var mentionedUser in message.Mentions.Users)
                //    content = Regex.Replace(content, $"<@!?{mentionedUser.Id}>", $"@{mentionedUser}");

                //// Channel mentions (<#id>)
                //foreach (var mentionedChannel in message.Mentions.Channels)
                //    content = content.Replace($"<#{mentionedChannel.Id}>", $"#{mentionedChannel.Name}");

                //// Role mentions (<@&id>)
                //foreach (var mentionedRole in message.Mentions.Roles)
                //    content = content.Replace($"<@&{mentionedRole.Id}>", $"@{mentionedRole.Name}");

                // Custom emojis (<:name:id>)
                content = Regex.Replace(content, "<(:.*?:)\\d*>", "$1");

                return content;
            }
        }
    }
}
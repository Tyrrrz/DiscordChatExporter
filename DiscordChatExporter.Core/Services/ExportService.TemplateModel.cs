using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using DiscordChatExporter.Core.Internal;
using DiscordChatExporter.Core.Models;
using Scriban.Runtime;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Services
{
    public partial class ExportService
    {
        private class TemplateModel
        {
            private readonly ExportFormat _format;
            private readonly ChannelChatLog _log;
            private readonly string _dateFormat;

            public TemplateModel(ExportFormat format, ChannelChatLog log, string dateFormat)
            {
                _format = format;
                _log = log;
                _dateFormat = dateFormat;

            }

            private string HtmlEncode(string str) => WebUtility.HtmlEncode(str);

            private string HtmlDecode(string str) => WebUtility.HtmlDecode(str);

            private string Format(IFormattable obj, string format) =>
                obj.ToString(format, CultureInfo.InvariantCulture);

            private string FormatDate(DateTime dateTime) => Format(dateTime, _dateFormat);

            private string FormatFileSize(long fileSize)
            {
                string[] units = {"B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"};
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

            private string FormatContentPlainText(string content)
            {
                // New lines
                content = content.Replace("\n", Environment.NewLine);

                // User mentions (<@id> and <@!id>)
                var mentionedUserIds = Regex.Matches(content, "<@!?(\\d+)>")
                    .Cast<Match>()
                    .Select(m => m.Groups[1].Value)
                    .ExceptBlank()
                    .ToArray();

                foreach (var mentionedUserId in mentionedUserIds)
                {
                    var mentionedUser = _log.Mentionables.GetUser(mentionedUserId);
                    content = Regex.Replace(content, $"<@!?{mentionedUserId}>", $"@{mentionedUser.FullName}");
                }

                // Channel mentions (<#id>)
                var mentionedChannelIds = Regex.Matches(content, "<#(\\d+)>")
                    .Cast<Match>()
                    .Select(m => m.Groups[1].Value)
                    .ExceptBlank()
                    .ToArray();

                foreach (var mentionedChannelId in mentionedChannelIds)
                {
                    var mentionedChannel = _log.Mentionables.GetChannel(mentionedChannelId);
                    content = content.Replace($"<#{mentionedChannelId}>", $"#{mentionedChannel.Name}");
                }

                // Role mentions (<@&id>)
                var mentionedRoleIds = Regex.Matches(content, "<@&(\\d+)>")
                    .Cast<Match>()
                    .Select(m => m.Groups[1].Value)
                    .ExceptBlank()
                    .ToArray();

                foreach (var mentionedRoleId in mentionedRoleIds)
                {
                    var mentionedRole = _log.Mentionables.GetRole(mentionedRoleId);
                    content = content.Replace($"<@&{mentionedRoleId}>", $"@{mentionedRole.Name}");
                }

                // Custom emojis (<:name:id>)
                content = Regex.Replace(content, "<(:.*?:)\\d*>", "$1");

                return content;
            }

            private string FormatContentHtml(string content, bool allowLinks = false)
            {
                // HTML-encode content
                content = HtmlEncode(content);

                // Encode multiline codeblocks (```text```)
                content = Regex.Replace(content,
                    @"```+(?:[^`]*?\n)?([^`]+)\n?```+",
                    m => $"\x1AM{m.Groups[1].Value.Base64Encode()}\x1AM");

                // Encode inline codeblocks (`text`)
                content = Regex.Replace(content,
                    @"`([^`]+)`",
                    m => $"\x1AI{m.Groups[1].Value.Base64Encode()}\x1AI");

                // Encode links
                if (allowLinks)
                {
                    content = Regex.Replace(content, @"\[(.*?)\]\((.*?)\)",
                        m => $"\x1AL{m.Groups[1].Value.Base64Encode()}|{m.Groups[2].Value.Base64Encode()}\x1AL");
                }

                // Encode URLs
                content = Regex.Replace(content,
                    @"(\b(?:(?:https?|ftp|file)://|www\.|ftp\.)(?:\([-a-zA-Z0-9+&@#/%?=~_|!:,\.\[\];]*\)|[-a-zA-Z0-9+&@#/%?=~_|!:,\.\[\];])*(?:\([-a-zA-Z0-9+&@#/%?=~_|!:,\.\[\];]*\)|[-a-zA-Z0-9+&@#/%=~_|$]))",
                    m => $"\x1AU{m.Groups[1].Value.Base64Encode()}\x1AU");

                // Process bold (**text**)
                content = Regex.Replace(content, @"(\*\*)(?=\S)(.+?[*_]*)(?<=\S)\1", "<b>$2</b>");

                // Process underline (__text__)
                content = Regex.Replace(content, @"(__)(?=\S)(.+?)(?<=\S)\1", "<u>$2</u>");

                // Process italic (*text* or _text_)
                content = Regex.Replace(content, @"(\*|_)(?=\S)(.+?)(?<=\S)\1", "<i>$2</i>");

                // Process strike through (~~text~~)
                content = Regex.Replace(content, @"(~~)(?=\S)(.+?)(?<=\S)\1", "<s>$2</s>");

                // Decode and process multiline codeblocks
                content = Regex.Replace(content, "\x1AM(.*?)\x1AM",
                    m => $"<div class=\"pre\">{m.Groups[1].Value.Base64Decode()}</div>");

                // Decode and process inline codeblocks
                content = Regex.Replace(content, "\x1AI(.*?)\x1AI",
                    m => $"<span class=\"pre\">{m.Groups[1].Value.Base64Decode()}</span>");

                // Decode and process links
                if (allowLinks)
                {
                    content = Regex.Replace(content, "\x1AL(.*?)\\|(.*?)\x1AL",
                        m => $"<a href=\"{m.Groups[2].Value.Base64Decode()}\">{m.Groups[1].Value.Base64Decode()}</a>");
                }

                // Decode and process URLs
                content = Regex.Replace(content, "\x1AU(.*?)\x1AU",
                    m => $"<a href=\"{m.Groups[1].Value.Base64Decode()}\">{m.Groups[1].Value.Base64Decode()}</a>");

                // Process new lines
                content = content.Replace("\n", "<br />");

                // Meta mentions (@everyone)
                content = content.Replace("@everyone", "<span class=\"mention\">@everyone</span>");

                // Meta mentions (@here)
                content = content.Replace("@here", "<span class=\"mention\">@here</span>");

                // User mentions (<@id> and <@!id>)
                var mentionedUserIds = Regex.Matches(content, "&lt;@!?(\\d+)&gt;")
                    .Cast<Match>()
                    .Select(m => m.Groups[1].Value)
                    .ExceptBlank()
                    .ToArray();

                foreach (var mentionedUserId in mentionedUserIds)
                {
                    var mentionedUser = _log.Mentionables.GetUser(mentionedUserId);
                    content = Regex.Replace(content, $"&lt;@!?{mentionedUserId}&gt;",
                        $"<span class=\"mention\" title=\"{HtmlEncode(mentionedUser.FullName)}\">" +
                        $"@{HtmlEncode(mentionedUser.Name)}" +
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
                    var mentionedChannel = _log.Mentionables.GetChannel(mentionedChannelId);
                    content = content.Replace($"&lt;#{mentionedChannelId}&gt;",
                        "<span class=\"mention\">" +
                        $"#{HtmlEncode(mentionedChannel.Name)}" +
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
                    var mentionedRole = _log.Mentionables.GetRole(mentionedRoleId);
                    content = content.Replace($"&lt;@&amp;{mentionedRoleId}&gt;",
                        "<span class=\"mention\">" +
                        $"@{HtmlEncode(mentionedRole.Name)}" +
                        "</span>");
                }

                // Custom emojis (<:name:id>)
                content = Regex.Replace(content, "&lt;(:.*?:)(\\d*)&gt;",
                    "<img class=\"emoji\" title=\"$1\" src=\"https://cdn.discordapp.com/emojis/$2.png\" />");

                return content;
            }

            private string FormatContentCsv(string content)
            {
                // New lines
                content = content.Replace("\n", ", ");

                // Escape quotes
                content = content.Replace("\"", "\"\"");

                // Escape commas and semicolons
                if (content.Contains(",") || content.Contains(";"))
                    content = $"\"{content}\"";

                // User mentions (<@id> and <@!id>)
                var mentionedUserIds = Regex.Matches(content, "<@!?(\\d+)>")
                    .Cast<Match>()
                    .Select(m => m.Groups[1].Value)
                    .ExceptBlank()
                    .ToArray();

                foreach (var mentionedUserId in mentionedUserIds)
                {
                    var mentionedUser = _log.Mentionables.GetUser(mentionedUserId);
                    content = Regex.Replace(content, $"<@!?{mentionedUserId}>", $"@{mentionedUser.FullName}");
                }

                // Channel mentions (<#id>)
                var mentionedChannelIds = Regex.Matches(content, "<#(\\d+)>")
                    .Cast<Match>()
                    .Select(m => m.Groups[1].Value)
                    .ExceptBlank()
                    .ToArray();

                foreach (var mentionedChannelId in mentionedChannelIds)
                {
                    var mentionedChannel = _log.Mentionables.GetChannel(mentionedChannelId);
                    content = content.Replace($"<#{mentionedChannelId}>", $"#{mentionedChannel.Name}");
                }

                // Role mentions (<@&id>)
                var mentionedRoleIds = Regex.Matches(content, "<@&(\\d+)>")
                    .Cast<Match>()
                    .Select(m => m.Groups[1].Value)
                    .ExceptBlank()
                    .ToArray();

                foreach (var mentionedRoleId in mentionedRoleIds)
                {
                    var mentionedRole = _log.Mentionables.GetRole(mentionedRoleId);
                    content = content.Replace($"<@&{mentionedRoleId}>", $"@{mentionedRole.Name}");
                }

                // Custom emojis (<:name:id>)
                content = Regex.Replace(content, "<(:.*?:)\\d*>", "$1");

                return content;
            }

            private string FormatContent(string content, bool allowLinks = false)
            {
                if (_format == ExportFormat.PlainText)
                    return FormatContentPlainText(content);

                if (_format == ExportFormat.HtmlDark)
                    return FormatContentHtml(content, allowLinks);

                if (_format == ExportFormat.HtmlLight)
                    return FormatContentHtml(content, allowLinks);

                if (_format == ExportFormat.Csv)
                    return FormatContentCsv(content);

                throw new ArgumentOutOfRangeException(nameof(_format));
            }

            public ScriptObject GetScriptObject()
            {
                // Create instance
                var scriptObject = new ScriptObject();

                // Import chat log
                scriptObject.Import(_log, TemplateMemberFilter, TemplateMemberRenamer);

                // Import functions
                scriptObject.Import(nameof(HtmlEncode), new Func<string, string>(HtmlEncode));
                scriptObject.Import(nameof(HtmlDecode), new Func<string, string>(HtmlDecode));
                scriptObject.Import(nameof(Format), new Func<IFormattable, string, string>(Format));
                scriptObject.Import(nameof(FormatDate), new Func<DateTime, string>(FormatDate));
                scriptObject.Import(nameof(FormatFileSize), new Func<long, string>(FormatFileSize));
                scriptObject.Import(nameof(FormatColor), new Func<Color, string>(FormatColor));
                scriptObject.Import(nameof(FormatContent), new Func<string, bool, string>(FormatContent));

                return scriptObject;
            }
        }
    }
}
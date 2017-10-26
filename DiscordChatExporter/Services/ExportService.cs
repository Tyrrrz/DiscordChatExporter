﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DiscordChatExporter.Models;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Services
{
    public partial class ExportService : IExportService
    {
        private readonly ISettingsService _settingsService;

        public ExportService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        private async Task ExportAsTextAsync(string filePath, ChannelChatLog log)
        {
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8, 128 * 1024))
            {
                // Generation info
                await writer.WriteLineAsync("https://github.com/Tyrrrz/DiscordChatExporter");
                await writer.WriteLineAsync();

                // Guild and channel info
                await writer.WriteLineAsync('='.Repeat(48));
                await writer.WriteLineAsync($"Guild: {log.Guild}");
                await writer.WriteLineAsync($"Channel: {log.Channel}");
                await writer.WriteLineAsync($"Messages: {log.TotalMessageCount:N0}");
                await writer.WriteLineAsync('='.Repeat(48));
                await writer.WriteLineAsync();

                // Chat log
                foreach (var group in log.MessageGroups)
                {
                    var timeStampFormatted = group.TimeStamp.ToString(_settingsService.DateFormat);
                    await writer.WriteLineAsync($"{group.Author}  [{timeStampFormatted}]");

                    // Messages
                    foreach (var message in group.Messages)
                    {
                        // Content
                        if (message.Content.IsNotBlank())
                        {
                            var contentFormatted = FormatMessageContentText(message);
                            await writer.WriteLineAsync(contentFormatted);
                        }

                        // Attachments
                        foreach (var attachment in message.Attachments)
                        {
                            await writer.WriteLineAsync(attachment.Url);
                        }
                    }

                    await writer.WriteLineAsync();
                }
            }
        }

        private async Task ExportAsHtmlAsync(string filePath, ChannelChatLog log, string css)
        {
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8, 128 * 1024))
            {
                // Generation info
                await writer.WriteLineAsync("<!-- https://github.com/Tyrrrz/DiscordChatExporter -->");

                // Html start
                await writer.WriteLineAsync("<!DOCTYPE html>");
                await writer.WriteLineAsync("<html lang=\"en\">");

                // HEAD
                await writer.WriteLineAsync("<head>");
                await writer.WriteLineAsync($"<title>{log.Guild} - {log.Channel}</title>");
                await writer.WriteLineAsync("<meta charset=\"utf-8\" />");
                await writer.WriteLineAsync("<meta name=\"viewport\" content=\"width=device-width\" />");
                await writer.WriteLineAsync($"<style>{css}</style>");
                await writer.WriteLineAsync("</head>");

                // Body start
                await writer.WriteLineAsync("<body>");

                // Guild and channel info
                await writer.WriteLineAsync("<div id=\"info\">");
                await writer.WriteLineAsync("<div class=\"info-left\">");
                await writer.WriteLineAsync($"<img class=\"guild-icon\" src=\"{log.Guild.IconUrl}\" />");
                await writer.WriteLineAsync("</div>"); // info-left
                await writer.WriteLineAsync("<div class=\"info-right\">");
                await writer.WriteLineAsync($"<div class=\"guild-name\">{log.Guild}</div>");
                await writer.WriteLineAsync($"<div class=\"channel-name\">{log.Channel}</div>");
                await writer.WriteLineAsync($"<div class=\"misc\">{log.TotalMessageCount:N0} messages</div>");
                await writer.WriteLineAsync("</div>"); // info-right
                await writer.WriteLineAsync("</div>"); // info

                // Chat log
                await writer.WriteLineAsync("<div id=\"log\">");
                foreach (var group in log.MessageGroups)
                {
                    await writer.WriteLineAsync("<div class=\"msg\">");
                    await writer.WriteLineAsync("<div class=\"msg-left\">");
                    await writer.WriteLineAsync($"<img class=\"msg-avatar\" src=\"{group.Author.AvatarUrl}\" />");
                    await writer.WriteLineAsync("</div>");

                    await writer.WriteLineAsync("<div class=\"msg-right\">");
                    await writer.WriteAsync($"<span class=\"msg-user\" title=\"{HtmlEncode(group.Author)}\">");
                    await writer.WriteAsync(HtmlEncode(group.Author.Name));
                    await writer.WriteLineAsync("</span>");
                    var timeStampFormatted = HtmlEncode(group.TimeStamp.ToString(_settingsService.DateFormat));
                    await writer.WriteLineAsync($"<span class=\"msg-date\">{timeStampFormatted}</span>");

                    // Messages
                    foreach (var message in group.Messages)
                    {
                        // Content
                        if (message.Content.IsNotBlank())
                        {
                            await writer.WriteLineAsync("<div class=\"msg-content\">");
                            var contentFormatted = FormatMessageContentHtml(message);
                            await writer.WriteAsync(contentFormatted);

                            // Edited timestamp
                            if (message.EditedTimeStamp != null)
                            {
                                var editedTimeStampFormatted =
                                    HtmlEncode(message.EditedTimeStamp.Value.ToString(_settingsService.DateFormat));
                                await writer.WriteAsync(
                                    $"<span class=\"msg-edited\" title=\"{editedTimeStampFormatted}\">(edited)</span>");
                            }

                            await writer.WriteLineAsync("</div>"); // msg-content
                        }

                        // Attachments
                        foreach (var attachment in message.Attachments)
                        {
                            if (attachment.Type == AttachmentType.Image)
                            {
                                await writer.WriteLineAsync("<div class=\"msg-attachment\">");
                                await writer.WriteLineAsync($"<a href=\"{attachment.Url}\">");
                                await writer.WriteLineAsync(
                                    $"<img class=\"msg-attachment\" src=\"{attachment.Url}\" />");
                                await writer.WriteLineAsync("</a>");
                                await writer.WriteLineAsync("</div>");
                            }
                            else
                            {
                                await writer.WriteLineAsync("<div class=\"msg-attachment\">");
                                await writer.WriteLineAsync($"<a href=\"{attachment.Url}\">");
                                var fileSizeFormatted = FormatFileSize(attachment.FileSize);
                                await writer.WriteLineAsync($"Attachment: {attachment.FileName} ({fileSizeFormatted})");
                                await writer.WriteLineAsync("</a>");
                                await writer.WriteLineAsync("</div>");
                            }
                        }
                    }
                    await writer.WriteLineAsync("</div>"); // msg-right
                    await writer.WriteLineAsync("</div>"); // msg
                }
                await writer.WriteLineAsync("</div>"); // log

                await writer.WriteLineAsync("</body>");
                await writer.WriteLineAsync("</html>");
            }
        }

        public Task ExportAsync(ExportFormat format, string filePath, ChannelChatLog log)
        {
            if (format == ExportFormat.PlainText)
            {
                return ExportAsTextAsync(filePath, log);
            }
            if (format == ExportFormat.HtmlDark)
            {
                var css = Program.GetResourceString("DiscordChatExporter.Resources.ExportService.DarkTheme.css");
                return ExportAsHtmlAsync(filePath, log, css);
            }
            if (format == ExportFormat.HtmlLight)
            {
                var css = Program.GetResourceString("DiscordChatExporter.Resources.ExportService.LightTheme.css");
                return ExportAsHtmlAsync(filePath, log, css);
            }

            throw new NotImplementedException();
        }
    }

    public partial class ExportService
    {
        private static string HtmlEncode(string str)
        {
            return WebUtility.HtmlEncode(str);
        }

        private static string HtmlEncode(object obj)
        {
            return WebUtility.HtmlEncode(obj.ToString());
        }

        private static string FormatFileSize(long fileSize)
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

        public static string FormatMessageContentText(Message message)
        {
            var content = message.Content;

            // New lines
            content = content.Replace("\n", Environment.NewLine);

            // User mentions (<@id>)
            content = Regex.Replace(content, "<@(\\d*)>",
                m =>
                {
                    var mentionedUser = message.MentionedUsers.First(u => u.Id == m.Groups[1].Value);
                    return $"@{mentionedUser}";
                });

            // Role mentions (<@&id>)
            content = Regex.Replace(content, "<@&(\\d*)>",
                m =>
                {
                    var mentionedRole = message.MentionedRoles.First(r => r.Id == m.Groups[1].Value);
                    return $"@{mentionedRole.Name}";
                });

            // Custom emojis (<:name:id>)
            content = Regex.Replace(content, "<(:.*?:)\\d*>", "$1");

            return content;
        }

        private static string FormatMessageContentHtml(Message message)
        {
            var content = message.Content;

            // Encode HTML
            content = HtmlEncode(content);

            // Pre multiline (```text```)
            content = Regex.Replace(content, "```+(?:[^`]*?\\n)?([^`]+)\\n?```+", "<div class=\"pre\">$1</div>");

            // Pre inline (`text`)
            content = Regex.Replace(content, "`([^`]+)`", "<span class=\"pre\">$1</span>");

            // URL links
            content = Regex.Replace(content, "((https?|ftp)://[^\\s/$.?#].[^\\s]*)",
                "<a href=\"$1\">$1</a>");

            // Bold (**text**)
            content = Regex.Replace(content, "\\*\\*([^\\*]*?)\\*\\*", "<b>$1</b>");

            // Italic (*text*)
            content = Regex.Replace(content, "\\*([^\\*]*?)\\*", "<i>$1</i>");

            // Underline (__text__)
            content = Regex.Replace(content, "__([^_]*?)__", "<u>$1</u>");

            // Italic (_text_)
            content = Regex.Replace(content, "_([^_]*?)_", "<i>$1</i>");

            // Strike through (~~text~~)
            content = Regex.Replace(content, "~~([^~]*?)~~", "<s>$1</s>");

            // New lines
            content = content.Replace("\n", "<br />");

            // Meta mentions (@everyone)
            content = content.Replace("@everyone", "<span class=\"mention\">@everyone</span>");

            // Meta mentions (@here)
            content = content.Replace("@here", "<span class=\"mention\">@here</span>");

            // User mentions (<@id>)
            content = Regex.Replace(content, "&lt;@(\\d*)&gt;",
                m =>
                {
                    var mentionedUser = message.MentionedUsers.First(u => u.Id == m.Groups[1].Value);
                    return $"<span class=\"mention\" title=\"{HtmlEncode(mentionedUser)}\">" +
                           $"@{HtmlEncode(mentionedUser.Name)}" +
                           "</span>";
                });

            // Role mentions (<@&id>)
            content = Regex.Replace(content, "&lt;@&amp;(\\d*)&gt;",
                m =>
                {
                    var mentionedRole = message.MentionedRoles.First(r => r.Id == m.Groups[1].Value);
                    return "<span class=\"mention\">" +
                           $"@{HtmlEncode(mentionedRole.Name)}" +
                           "</span>";
                });

            // Custom emojis (<:name:id>)
            content = Regex.Replace(content, "&lt;(:.*?:)(\\d*)&gt;",
                "<img class=\"emoji\" title=\"$1\" src=\"https://cdn.discordapp.com/emojis/$2.png\" />");

            return content;
        }
    }
}
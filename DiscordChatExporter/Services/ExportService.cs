using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Resources;
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

        public async Task ExportAsTextAsync(string filePath, ChannelChatLog log)
        {
            var dateFormat = _settingsService.DateFormat;

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
                    var timeStampFormatted = group.TimeStamp.ToString(dateFormat);
                    await writer.WriteLineAsync($"{group.Author}  [{timeStampFormatted}]");

                    // Messages
                    foreach (var message in group.Messages)
                    {
                        // Content
                        if (message.Content.IsNotBlank())
                        {
                            var contentFormatted = message.Content.Replace("\n", Environment.NewLine);
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

        public async Task ExportAsHtmlAsync(string filePath, ChannelChatLog log, Theme theme)
        {
            var themeCss = GetThemeCss(theme);
            var dateFormat = _settingsService.DateFormat;

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
                await writer.WriteLineAsync($"<style>{themeCss}</style>");
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
                    var timeStampFormatted = HtmlEncode(group.TimeStamp.ToString(dateFormat));
                    await writer.WriteLineAsync($"<span class=\"msg-date\">{timeStampFormatted}</span>");

                    // Messages
                    foreach (var message in group.Messages)
                    {
                        // Content
                        if (message.Content.IsNotBlank())
                        {
                            await writer.WriteLineAsync("<div class=\"msg-content\">");
                            var contentFormatted = FormatMessageContentHtml(message.Content);
                            await writer.WriteAsync(contentFormatted);

                            // Edited timestamp
                            if (message.EditedTimeStamp != null)
                            {
                                var editedTimeStampFormatted =
                                    HtmlEncode(message.EditedTimeStamp.Value.ToString(dateFormat));
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
    }

    public partial class ExportService
    {
        private static string GetThemeCss(Theme theme)
        {
            var resourcePath = $"DiscordChatExporter.Resources.ExportService.{theme}Theme.css";

            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(resourcePath);
            if (stream == null)
                throw new MissingManifestResourceException("Could not find style resource");

            using (stream)
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

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

        private static string FormatMessageContentHtml(string content)
        {
            // Encode HTML
            content = HtmlEncode(content);

            // Preformatted div
            content = Regex.Replace(content, "```+(?:[^`]*?\\n)?([^`]+)\\n?```+",
                m => "<div class=\"pre\">" + m.Groups[1].Value + "</div>");

            // Preformatted span
            content = Regex.Replace(content, "`([^`]+)`",
                m => "<span class=\"pre\">" + m.Groups[1].Value + "</span>");

            // Links from URLs
            content = Regex.Replace(content, "((https?|ftp)://[^\\s/$.?#].[^\\s]*)",
                "<a href=\"$1\">$1</a>");

            // Bold
            content = Regex.Replace(content, "\\*\\*([^\\*]*?)\\*\\*", "<b>$1</b>");

            // Italic
            content = Regex.Replace(content, "\\*([^\\*]*?)\\*", "<i>$1</i>");

            // Underline
            content = Regex.Replace(content, "__([^_]*?)__", "<u>$1</u>");

            // Strike through
            content = Regex.Replace(content, "~~([^~]*?)~~", "<s>$1</s>");

            // New lines
            content = content.Replace("\n", "<br />");

            return content;
        }
    }
}
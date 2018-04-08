using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Services
{
    public partial class ExportService
    {
        private string FormatMessageContentHtml(Message message)
        {
            // A lot of these regexes were inspired by or taken from MarkdownSharp

            var content = message.Content;

            // HTML-encode content
            content = HtmlEncode(content);

            // Encode multiline codeblocks (```text```)
            content = Regex.Replace(content,
                @"```+(?:[^`]*?\n)?([^`]+)\n?```+",
                m => $"\x1AM{Base64Encode(m.Groups[1].Value)}\x1AM");

            // Encode inline codeblocks (`text`)
            content = Regex.Replace(content,
                @"`([^`]+)`",
                m => $"\x1AI{Base64Encode(m.Groups[1].Value)}\x1AI");

            // Encode URLs
            content = Regex.Replace(content,
                @"((https?|ftp)://[-a-zA-Z0-9+&@#/%?=~_|!:,\.\[\]\(\);]*[-a-zA-Z0-9+&@#/%=~_|\[\])])(?=$|\W)",
                m => $"\x1AL{Base64Encode(m.Groups[1].Value)}\x1AL");

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
                m => $"<div class=\"pre\">{Base64Decode(m.Groups[1].Value)}</div>");

            // Decode and process inline codeblocks
            content = Regex.Replace(content, "\x1AI(.*?)\x1AI",
                m => $"<span class=\"pre\">{Base64Decode(m.Groups[1].Value)}</span>");

            // Decode and process URLs
            content = Regex.Replace(content, "\x1AL(.*?)\x1AL",
                m => $"<a href=\"{Base64Decode(m.Groups[1].Value)}\">{Base64Decode(m.Groups[1].Value)}</a>");

            // New lines
            content = content.Replace("\n", "<br />");

            // Meta mentions (@everyone)
            content = content.Replace("@everyone", "<span class=\"mention\">@everyone</span>");

            // Meta mentions (@here)
            content = content.Replace("@here", "<span class=\"mention\">@here</span>");

            // User mentions (<@id> and <@!id>)
            foreach (var mentionedUser in message.MentionedUsers)
            {
                content = Regex.Replace(content, $"&lt;@!?{mentionedUser.Id}&gt;",
                    $"<span class=\"mention\" title=\"{HtmlEncode(mentionedUser.FullName)}\">" +
                    $"@{HtmlEncode(mentionedUser.Name)}" +
                    "</span>");
            }

            // Role mentions (<@&id>)
            foreach (var mentionedRole in message.MentionedRoles)
            {
                content = content.Replace($"&lt;@&amp;{mentionedRole.Id}&gt;",
                    "<span class=\"mention\">" +
                    $"@{HtmlEncode(mentionedRole.Name)}" +
                    "</span>");
            }

            // Channel mentions (<#id>)
            foreach (var mentionedChannel in message.MentionedChannels)
            {
                content = content.Replace($"&lt;#{mentionedChannel.Id}&gt;",
                    "<span class=\"mention\">" +
                    $"#{HtmlEncode(mentionedChannel.Name)}" +
                    "</span>");
            }

            // Custom emojis (<:name:id>)
            content = Regex.Replace(content, "&lt;(:.*?:)(\\d*)&gt;",
                "<img class=\"emoji\" title=\"$1\" src=\"https://cdn.discordapp.com/emojis/$2.png\" />");

            return content;
        }

        private async Task ExportAsHtmlAsync(ChannelChatLog log, TextWriter output, string css)
        {
            // Generation info
            await output.WriteLineAsync("<!-- https://github.com/Tyrrrz/DiscordChatExporter -->");

            // Html start
            await output.WriteLineAsync("<!DOCTYPE html>");
            await output.WriteLineAsync("<html lang=\"en\">");

            // HEAD
            await output.WriteLineAsync("<head>");
            await output.WriteLineAsync($"<title>{log.Guild.Name} - {log.Channel.Name}</title>");
            await output.WriteLineAsync("<meta charset=\"utf-8\" />");
            await output.WriteLineAsync("<meta name=\"viewport\" content=\"width=device-width\" />");
            await output.WriteLineAsync($"<style>{css}</style>");
            await output.WriteLineAsync("</head>");

            // Body start
            await output.WriteLineAsync("<body>");

            // Guild and channel info
            await output.WriteLineAsync("<div id=\"info\">");
            await output.WriteLineAsync("<div class=\"info-left\">");
            await output.WriteLineAsync($"<img class=\"guild-icon\" src=\"{log.Guild.IconUrl}\" />");
            await output.WriteLineAsync("</div>"); // info-left
            await output.WriteLineAsync("<div class=\"info-right\">");
            await output.WriteLineAsync($"<div class=\"guild-name\">{log.Guild.Name}</div>");
            await output.WriteLineAsync($"<div class=\"channel-name\">{log.Channel.Name}</div>");
            await output.WriteLineAsync($"<div class=\"channel-topic\">{log.Channel.Topic}</div>");
            await output.WriteLineAsync(
                $"<div class=\"channel-messagecount\">{log.TotalMessageCount:N0} messages</div>");
            await output.WriteLineAsync("</div>"); // info-right
            await output.WriteLineAsync("</div>"); // info

            // Chat log
            await output.WriteLineAsync("<div id=\"log\">");
            foreach (var group in log.MessageGroups)
            {
                await output.WriteLineAsync("<div class=\"msg\">");
                await output.WriteLineAsync("<div class=\"msg-left\">");
                await output.WriteLineAsync($"<img class=\"msg-avatar\" src=\"{group.Author.AvatarUrl}\" />");
                await output.WriteLineAsync("</div>");

                await output.WriteLineAsync("<div class=\"msg-right\">");
                await output.WriteAsync(
                    $"<span class=\"msg-user\" title=\"{HtmlEncode(group.Author.FullName)}\">");
                await output.WriteAsync(HtmlEncode(group.Author.Name));
                await output.WriteLineAsync("</span>");
                var timeStampFormatted = HtmlEncode(group.TimeStamp.ToString(_settingsService.DateFormat));
                await output.WriteLineAsync($"<span class=\"msg-date\">{timeStampFormatted}</span>");

                // Messages
                foreach (var message in group.Messages)
                {
                    // Content
                    if (message.Content.IsNotBlank())
                    {
                        await output.WriteLineAsync("<div class=\"msg-content\">");
                        var contentFormatted = FormatMessageContentHtml(message);
                        await output.WriteAsync(contentFormatted);

                        // Edited timestamp
                        if (message.EditedTimeStamp != null)
                        {
                            var editedTimeStampFormatted =
                                HtmlEncode(message.EditedTimeStamp.Value.ToString(_settingsService.DateFormat));
                            await output.WriteAsync(
                                $"<span class=\"msg-edited\" title=\"{editedTimeStampFormatted}\">(edited)</span>");
                        }

                        await output.WriteLineAsync("</div>"); // msg-content
                    }

                    // Attachments
                    foreach (var attachment in message.Attachments)
                    {
                        if (attachment.Type == AttachmentType.Image)
                        {
                            await output.WriteLineAsync("<div class=\"msg-attachment\">");
                            await output.WriteLineAsync($"<a href=\"{attachment.Url}\">");
                            await output.WriteLineAsync($"<img class=\"msg-attachment\" src=\"{attachment.Url}\" />");
                            await output.WriteLineAsync("</a>");
                            await output.WriteLineAsync("</div>");
                        }
                        else
                        {
                            await output.WriteLineAsync("<div class=\"msg-attachment\">");
                            await output.WriteLineAsync($"<a href=\"{attachment.Url}\">");
                            var fileSizeFormatted = FormatFileSize(attachment.FileSize);
                            await output.WriteLineAsync($"Attachment: {attachment.FileName} ({fileSizeFormatted})");
                            await output.WriteLineAsync("</a>");
                            await output.WriteLineAsync("</div>");
                        }
                    }
                }

                await output.WriteLineAsync("</div>"); // msg-right
                await output.WriteLineAsync("</div>"); // msg
            }

            await output.WriteLineAsync("</div>"); // log

            await output.WriteLineAsync("</body>");
            await output.WriteLineAsync("</html>");
        }
    }
}
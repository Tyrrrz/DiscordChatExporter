using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;
using Tyrrrz.Extensions;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordChatExporter.Core.Services
{
    public partial class ExportService
    {
        private string MarkdownToHtml(string content, IMentionable mentionable = null, bool allowLinks = false)
        {
            // A lot of these regexes were inspired by or taken from MarkdownSharp

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
                @"(\b(?:(?:https?|ftp|file)://|www\.|ftp\.)(?:\([-a-zA-Z0-9+&@#/%?=~_|!:,\.\[\];]*\)|[-a-zA-Z0-9+&@#/%?=~_|!:,\.\[\];])*(?:\([-a-zA-Z0-9+&@#/%?=~_|!:,\.\[\];]*\)|[-a-zA-Z0-9+&@#/%=~_|$]))",
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

            if (allowLinks)
            {
                content = Regex.Replace(content, "\\[([^\\]]+)\\]\\(\x1AL(.*?)\x1AL\\)",
                    m => $"<a href=\"{Base64Decode(m.Groups[2].Value)}\">{m.Groups[1].Value}</a>");
            }

            // Decode and process URLs
            content = Regex.Replace(content, "\x1AL(.*?)\x1AL",
                m => $"<a href=\"{Base64Decode(m.Groups[1].Value)}\">{Base64Decode(m.Groups[1].Value)}</a>");

            // New lines
            content = content.Replace("\n", "<br />");

            // Meta mentions (@everyone)
            content = content.Replace("@everyone", "<span class=\"mention\">@everyone</span>");

            // Meta mentions (@here)
            content = content.Replace("@here", "<span class=\"mention\">@here</span>");

            if (mentionable != null)
            {
                // User mentions (<@id> and <@!id>)
                foreach (var mentionedUser in mentionable.MentionedUsers)
                {
                    content = Regex.Replace(content, $"&lt;@!?{mentionedUser.Id}&gt;",
                        $"<span class=\"mention\" title=\"{HtmlEncode(mentionedUser.FullName)}\">" +
                        $"@{HtmlEncode(mentionedUser.Name)}" +
                        "</span>");
                }

                // Role mentions (<@&id>)
                foreach (var mentionedRole in mentionable.MentionedRoles)
                {
                    content = content.Replace($"&lt;@&amp;{mentionedRole.Id}&gt;",
                        "<span class=\"mention\">" +
                        $"@{HtmlEncode(mentionedRole.Name)}" +
                        "</span>");
                }

                // Channel mentions (<#id>)
                foreach (var mentionedChannel in mentionable.MentionedChannels)
                {
                    content = content.Replace($"&lt;#{mentionedChannel.Id}&gt;",
                        "<span class=\"mention\">" +
                        $"#{HtmlEncode(mentionedChannel.Name)}" +
                        "</span>");
                }
            }

            // Custom emojis (<:name:id>)
            content = Regex.Replace(content, "&lt;(:.*?:)(\\d*)&gt;",
                "<img class=\"emoji\" title=\"$1\" src=\"https://cdn.discordapp.com/emojis/$2.png\" />");

            return content;
        }

        private string FormatMessageContentHtml(Message message)
        {
            return MarkdownToHtml(message.Content, message);
        }

        // The code used to convert embeds to html was based heavily off of the Embed Visualizer project, from this file:
        // https://github.com/leovoel/embed-visualizer/blob/master/src/components/embed.jsx

        private string EmbedColorPillToHtml(Color? color)
        {
            string backgroundColor = "";

            if (color != null)
                backgroundColor = $"rgba({color?.R},{color?.G},{color?.B},1)";

            return $"<div class='embed-color-pill' style='background-color: {backgroundColor}'></div>";
        }

        private string EmbedTitleToHtml(string title, string url)
        {
            if (title == null)
                return null;

            string computed = $"<div class='embed-title'>{MarkdownToHtml(title)}</div>";
            if (url != null)
                computed = $"<a target='_blank' rel='noreferrer' href='{url}' class='embed-title'>{MarkdownToHtml(title)}</a>";

            return computed;
        }

        private string EmbedDescriptionToHtml(string content, IMentionable mentionable)
        {
            if (content == null)
                return null;

            return $"<div class='embed-description markup'>{MarkdownToHtml(content, mentionable, true)}</div>";
        }

        private string EmbedAuthorToHtml(string name, string url, string icon_url)
        {
            if (name == null)
                return null;

            string authorName = null;
            if (name != null)
            {
                authorName = $"<span class='embed-author-name'>{name}</span>";
                if (url != null)
                    authorName = $"<a target='_blank' rel='noreferrer' href='{url}' class='embed-author-name'>{name}</a>";
            }

            string authorIcon = icon_url != null ? $"<img src='{icon_url}' role='presentation' class='embed-author-icon' />" : null;

            return $"<div class='embed-author'>{authorIcon}{authorName}</div>";
        }

        private string EmbedFieldToHtml(string name, string value, bool? inline, IMentionable mentionable)
        {
            if (name == null && value == null)
                return null;

            string cls = "embed-field" + (inline == true ? " embed-field-inline" : "");

            string fieldName = name != null ? $"<div class='embed-field-name'>{MarkdownToHtml(name)}</div>" : null;
            string fieldValue = value != null ? $"<div class='embed-field-value markup'>{MarkdownToHtml(value, mentionable, true)}</div>" : null;

            return $"<div class='{cls}'>{fieldName}{fieldValue}</div>";
        }

        private string EmbedThumbnailToHtml(string url)
        {
            if (url == null)
                return null;

            return $@"
              <img
                  src = '{url}'
                  role = 'presentation'
                  class='embed-rich-thumb'
                  style='max-width: 80px; max-height: 80px'
              />";
        }

        private string EmbedImageToHtml(string url)
        {
            if (url == null)
                return null;

            return $"<a class='embed-thumbnail embed-thumbnail-rich'><img class='image' role='presentation' src='{url}' /></a>";
        }

        private string EmbedFooterToHtml(DateTime? timestamp, string text, string icon_url)
        {
            if (text == null && timestamp == null)
                return null;

            // format: ddd MMM Do, YYYY [at] h:mm A

            string time = timestamp != null ? HtmlEncode(timestamp?.ToString(_settingsService.DateFormat)) : null;

            string footerText = string.Join(" | ", new List<string> { text, time }.Where(s => s != null));
            string footerIcon = text != null && icon_url != null
                ? $"<img src='{icon_url}' class='embed-footer-icon' role='presentation' width='20' height='20' />"
                : null;

            return $"<div>{footerIcon}<span class='embed-footer'>{footerText}</span></div>";
        }

        private string EmbedFieldsToHtml(IReadOnlyList<EmbedField> fields, IMentionable mentionable)
        {
            if (fields.Count == 0)
                return null;

            return $"<div class='embed-fields'>{string.Join("", fields.Select(f => EmbedFieldToHtml(f.Name, f.Value, f.Inline, mentionable)))}</div>";
        }

        private string FormatEmbedHtml(Embed embed)
        {
            return $@"
                <div class='accessory'>
                  <div class='embed-wrapper'>
                    {EmbedColorPillToHtml(embed.Color)}
                    <div class='embed embed-rich'>
                      <div class='embed-content'>
                        <div class='embed-content-inner'>
                          {EmbedAuthorToHtml(embed.Author?.Name, embed.Author?.Url, embed.Author?.IconUrl)}
                          {EmbedTitleToHtml(embed.Title, embed.Url)}
                          {EmbedDescriptionToHtml(embed.Description, embed)}
                          {EmbedFieldsToHtml(embed.Fields, embed)}
                        </div>
                        {EmbedThumbnailToHtml(embed.Thumbnail?.Url)}
                      </div>
                      {EmbedImageToHtml(embed.Image?.Url)}
                      {EmbedFooterToHtml(embed.TimeStamp, embed.Footer?.Text, embed.Footer?.IconUrl)}
                    </div>
                  </div>
                </div>";
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

                    // Embeds
                    foreach (var embed in message.Embeds)
                    {
                        var contentFormatted = FormatEmbedHtml(embed);
                        await output.WriteAsync(contentFormatted);
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
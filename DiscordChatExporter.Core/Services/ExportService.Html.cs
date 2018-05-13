using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;
using Tyrrrz.Extensions;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DiscordChatExporter.Core.Internal;
using DiscordChatExporter.Core.Models.Embeds;

namespace DiscordChatExporter.Core.Services
{
    public partial class ExportService
    {
        private string MarkdownToHtml(string markdown, MentionsContainer mentions = null, bool allowLinks = false)
        {
            // HTML-encode content
            markdown = markdown.HtmlEncode();

            // Encode multiline codeblocks (```text```)
            markdown = Regex.Replace(markdown,
                @"```+(?:.*?\n)?(.+?)\n?```+",
                m => $"\x1AM{m.Groups[1].Value.Base64Encode()}\x1AM");

            // Encode inline codeblocks (`text`)
            markdown = Regex.Replace(markdown,
                @"`(.+?)`",
                m => $"\x1AI{m.Groups[1].Value.Base64Decode()}\x1AI");

            // Encode links
            if (allowLinks)
            {
                markdown = Regex.Replace(markdown, @"\[(.*?)\]\((.*?)\)",
                    m => $"\x1AL{m.Groups[1].Value.Base64Encode()}|{m.Groups[2].Value.Base64Encode()}\x1AL");
            }

            // Encode URLs
            markdown = Regex.Replace(markdown,
                @"(\b(?:(?:https?|ftp|file)://|www\.|ftp\.)(?:\([-a-zA-0-9+&@#/%?=~_|!:,\.\[\];]*\)|[-a-zA-Z0-9+&@#/%?=~_|!:,\.\[\];])*(?:\([-a-zA-Z0-9+&@#/%?=~_|!:,\.\[\];]*\)|[-a-zA-Z0-9+&@#/%=~_|$]))",
                m => $"\x1AU{m.Groups[1].Value.Base64Encode()}\x1AU");

            // Process bold (**text**)
            markdown = Regex.Replace(markdown, @"(\*\*)(?=\S)(.+?[*_]*)(?<=\S)\1", "<b>$2</b>");

            // Process underline (__text__)
            markdown = Regex.Replace(markdown, @"(__)(?=\S)(.+?)(?<=\S)\1", "<u>$2</u>");

            // Process italic (*text* or _text_)
            markdown = Regex.Replace(markdown, @"(\*|_)(?=\S)(.+?)(?<=\S)\1", "<i>$2</i>");

            // Process strike through (~~text~~)
            markdown = Regex.Replace(markdown, @"(~~)(?=\S)(.+?)(?<=\S)\1", "<s>$2</s>");

            // Decode and process multiline codeblocks
            markdown = Regex.Replace(markdown, "\x1AM(.*?)\x1AM",
                m => $"<div class=\"pre\">{m.Groups[1].Value.Base64Decode()}</div>");

            // Decode and process inline codeblocks
            markdown = Regex.Replace(markdown, "\x1AI(.*?)\x1AI",
                m => $"<span class=\"pre\">{m.Groups[1].Value.Base64Decode()}</span>");

            // Decode and process links
            if (allowLinks)
            {
                markdown = Regex.Replace(markdown, "\x1AL(.*?)|(.*?)\x1AL",
                    m => $"<a href=\"{m.Groups[2].Value.Base64Decode()}\">{m.Groups[1].Value.Base64Decode()}</a>");
            }

            // Decode and process URLs
            markdown = Regex.Replace(markdown, "\x1AU(.*?)\x1AU",
                m => $"<a href=\"{m.Groups[1].Value.Base64Decode()}\">{m.Groups[1].Value.Base64Decode()}</a>");

            // New lines
            markdown = markdown.Replace("\n", "<br />");

            // Meta mentions (@everyone)
            markdown = markdown.Replace("@everyone", "<span class=\"mention\">@everyone</span>");

            // Meta mentions (@here)
            markdown = markdown.Replace("@here", "<span class=\"mention\">@here</span>");

            // Mentions
            if (mentions != null)
            {
                // User mentions (<@id> and <@!id>)
                foreach (var mentionedUser in mentions.Users)
                {
                    markdown = Regex.Replace(markdown, $"&lt;@!?{mentionedUser.Id}&gt;",
                        $"<span class=\"mention\" title=\"{mentionedUser.FullName.HtmlEncode()}\">" +
                        $"@{mentionedUser.Name.HtmlEncode()}" +
                        "</span>");
                }

                // Role mentions (<@&id>)
                foreach (var mentionedRole in mentions.Roles)
                {
                    markdown = markdown.Replace($"&lt;@&amp;{mentionedRole.Id}&gt;",
                        "<span class=\"mention\">" +
                        $"@{mentionedRole.Name.HtmlEncode()}" +
                        "</span>");
                }

                // Channel mentions (<#id>)
                foreach (var mentionedChannel in mentions.Channels)
                {
                    markdown = markdown.Replace($"&lt;#{mentionedChannel.Id}&gt;",
                        "<span class=\"mention\">" +
                        $"#{mentionedChannel.Name.HtmlEncode()}" +
                        "</span>");
                }
            }

            // Custom emojis (<:name:id>)
            markdown = Regex.Replace(markdown, "&lt;(:.*?:)(\\d*)&gt;",
                "<img class=\"emoji\" title=\"$1\" src=\"https://cdn.discordapp.com/emojis/$2.png\" />");

            return markdown;
        }

        private string FormatMessageContentHtml(Message message)
        {
            return MarkdownToHtml(message.Content, message.Mentions);
        }

        // The code used to convert embeds to html was based heavily off of the Embed Visualizer project, from this file:
        // https://github.com/leovoel/embed-visualizer/blob/master/src/components/embed.jsx

        private string EmbedTitleToHtml(string title, string url)
        {
            if (title == null)
                return null;

            string computed = $"<div class='embed-title'>{MarkdownToHtml(title)}</div>";
            if (url != null)
                computed =
                    $"<a target='_blank' rel='noreferrer' href='{url}' class='embed-title'>{MarkdownToHtml(title)}</a>";

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
                    authorName =
                        $"<a target='_blank' rel='noreferrer' href='{url}' class='embed-author-name'>{name}</a>";
            }

            string authorIcon = icon_url != null
                ? $"<img src='{icon_url}' role='presentation' class='embed-author-icon' />"
                : null;

            return $"<div class='embed-author'>{authorIcon}{authorName}</div>";
        }

        private string EmbedFieldToHtml(string name, string value, bool? inline, IMentionable mentionable)
        {
            if (name == null && value == null)
                return null;

            string cls = "embed-field" + (inline == true ? " embed-field-inline" : "");

            string fieldName = name != null ? $"<div class='embed-field-name'>{MarkdownToHtml(name)}</div>" : null;
            string fieldValue = value != null
                ? $"<div class='embed-field-value markup'>{MarkdownToHtml(value, mentionable, true)}</div>"
                : null;

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

            return
                $"<a class='embed-thumbnail embed-thumbnail-rich'><img class='image' role='presentation' src='{url}' /></a>";
        }

        private string EmbedFooterToHtml(DateTime? timestamp, string text, string icon_url)
        {
            if (text == null && timestamp == null)
                return null;

            // format: ddd MMM Do, YYYY [at] h:mm A

            string time = timestamp != null ? HtmlEncode(timestamp?.ToString(_settingsService.DateFormat)) : null;

            string footerText = string.Join(" | ", new List<string> {text, time}.Where(s => s != null));
            string footerIcon = text != null && icon_url != null
                ? $"<img src='{icon_url}' class='embed-footer-icon' role='presentation' width='20' height='20' />"
                : null;

            return $"<div>{footerIcon}<span class='embed-footer'>{footerText}</span></div>";
        }

        private string FormatEmbedHtml(Embed embed)
        {
            var fieldsFormatted = embed.Fields.NotNullAndAny()
                ? $"<div class='embed-fields'>{string.Join("", embed.Fields.Select(f => EmbedFieldToHtml(f.Name, f.Value, f.Inline, embed.Mentions)))}</div>"
                : "";

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
                          {fieldsFormatted}
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
                    $"<span class=\"msg-user\" title=\"{group.Author.FullName.HtmlEncode()}\">");
                await output.WriteAsync(group.Author.Name.HtmlEncode());
                await output.WriteLineAsync("</span>");
                var timeStampFormatted = group.TimeStamp.ToString(_settingsService.DateFormat).HtmlEncode();
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
                                message.EditedTimeStamp.Value.ToString(_settingsService.DateFormat).HtmlEncode();
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
                        await output.WriteLineAsync("<div class=\"accessory\">"); // TODO: remove?
                        await output.WriteLineAsync("<div class=\"embed-wrapper\">");

                        // Color pill
                        await output.WriteLineAsync(
                            $"<div class=\'embed-color-pill\' style=\'background-color: rgba({embed.Color?.R},{embed.Color?.G},{embed.Color?.B},1)\'></div>");

                        // Embed
                        await output.WriteLineAsync("<div class=\"embed embed-rich\">");

                        // Embed content
                        await output.WriteLineAsync("<div class=\"embed-content\">");
                        await output.WriteLineAsync("<div class=\"embed-content-inner\">");




                        await output.WriteLineAsync("</div>"); // embed-content
                        await output.WriteLineAsync("</div>"); // embed-content-inner

                        await output.WriteLineAsync("</div>"); // embed
                        await output.WriteLineAsync("</div>"); // embed-wrapper
                        await output.WriteLineAsync("</div>"); // accessory
                    }
                }

                await output.WriteLineAsync("</div>"); // msg-right
                await output.WriteLineAsync("</div>"); // msg
            }

            await output.WriteLineAsync("</div>"); // log

            await output.WriteLineAsync("</body>");
            await output.WriteLineAsync("</html>");
        }

        private Task ExportAsHtmlDarkAsync(ChannelChatLog log, TextWriter output)
        {
            // Get css
            var sharedCss = Assembly.GetExecutingAssembly()
                .GetManifestResourceString("DiscordChatExporter.Core.Resources.ExportService.Shared.css");
            var themeCss = Assembly.GetExecutingAssembly()
                .GetManifestResourceString("DiscordChatExporter.Core.Resources.ExportService.DarkTheme.css");
            var combinedCss = sharedCss + Environment.NewLine + themeCss;

            // Export
            return ExportAsHtmlAsync(log, output, combinedCss);
        }

        private Task ExportAsHtmlLightAsync(ChannelChatLog log, TextWriter output)
        {
            // Get css
            var sharedCss = Assembly.GetExecutingAssembly()
                .GetManifestResourceString("DiscordChatExporter.Core.Resources.ExportService.Shared.css");
            var themeCss = Assembly.GetExecutingAssembly()
                .GetManifestResourceString("DiscordChatExporter.Core.Resources.ExportService.LightTheme.css");
            var combinedCss = sharedCss + Environment.NewLine + themeCss;

            // Export
            return ExportAsHtmlAsync(log, output, combinedCss);
        }
    }
}
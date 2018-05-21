using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Models;
using Tyrrrz.Extensions;
using System;
using System.Reflection;
using DiscordChatExporter.Core.Internal;

namespace DiscordChatExporter.Core.Services
{
    public partial class ExportService
    {
        private string FormatMentionsHtml(string content, MentionsContainer mentions)
        {
            // Meta mentions (@everyone)
            content = content.Replace("@everyone", "<span class=\"mention\">@everyone</span>");

            // Meta mentions (@here)
            content = content.Replace("@here", "<span class=\"mention\">@here</span>");

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

            return content;
        }

        private string FormatEmojisHtml(string content)
        {
            // Custom emojis (<:name:id>)
            content = Regex.Replace(content, "&lt;(:.*?:)(\\d*)&gt;",
                "<img class=\"emoji\" title=\"$1\" src=\"https://cdn.discordapp.com/emojis/$2.png\" />");

            return content;
        }

        private string FormatMessageContentHtml(Message message)
        {
            var content = message.Content;

            // Convert content markdown to HTML
            content = MarkdownProcessor.ToHtml(content);

            // Format mentions
            content = FormatMentionsHtml(content, message.Mentions);

            // Format emojis
            content = FormatEmojisHtml(content);

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
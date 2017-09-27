using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using DiscordChatExporter.Models;
using HtmlAgilityPack;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Services
{
    public partial class ExportService : IExportService
    {
        public void Export(string filePath, ChannelChatLog channelChatLog, Theme theme)
        {
            var doc = GetTemplate();
            var style = GetStyle(theme);

            // Set theme
            var themeHtml = doc.GetElementbyId("theme");
            themeHtml.InnerHtml = style;

            // Title
            var titleHtml = doc.DocumentNode.Element("html").Element("head").Element("title");
            titleHtml.InnerHtml = $"{channelChatLog.Guild.Name} - {channelChatLog.Channel.Name}";

            // Info
            var infoHtml = doc.GetElementbyId("info");
            var infoLeftHtml = infoHtml.AppendChild(HtmlNode.CreateNode("<div class=\"info-left\"></div>"));
            infoLeftHtml.AppendChild(HtmlNode.CreateNode(
                $"<img class=\"guild-icon\" src=\"{channelChatLog.Guild.IconUrl}\" />"));
            var infoRightHtml = infoHtml.AppendChild(HtmlNode.CreateNode("<div class=\"info-right\"></div>"));
            infoRightHtml.AppendChild(HtmlNode.CreateNode(
                $"<div class=\"guild-name\">{channelChatLog.Guild.Name}</div>"));
            infoRightHtml.AppendChild(HtmlNode.CreateNode(
                $"<div class=\"channel-name\">{channelChatLog.Channel.Name}</div>"));
            infoRightHtml.AppendChild(HtmlNode.CreateNode(
                $"<div class=\"misc\">{channelChatLog.Messages.Count:N0} messages</div>"));

            // Log
            var logHtml = doc.GetElementbyId("log");
            var messageGroups = GroupMessages(channelChatLog.Messages);
            foreach (var messageGroup in messageGroups)
            {
                // Container
                var messageHtml = logHtml.AppendChild(HtmlNode.CreateNode("<div class=\"msg\"></div>"));

                // Left
                var messageLeftHtml = messageHtml.AppendChild(HtmlNode.CreateNode("<div class=\"msg-left\"></div>"));

                // Avatar
                messageLeftHtml.AppendChild(
                    HtmlNode.CreateNode($"<img class=\"msg-avatar\" src=\"{messageGroup.Author.AvatarUrl}\" />"));

                // Right
                var messageRightHtml = messageHtml.AppendChild(HtmlNode.CreateNode("<div class=\"msg-right\"></div>"));

                // Author
                var authorName = HtmlDocument.HtmlEncode(messageGroup.Author.Name);
                messageRightHtml.AppendChild(HtmlNode.CreateNode($"<span class=\"msg-user\">{authorName}</span>"));

                // Date
                var timeStamp = HtmlDocument.HtmlEncode(messageGroup.TimeStamp.ToString("g"));
                messageRightHtml.AppendChild(HtmlNode.CreateNode($"<span class=\"msg-date\">{timeStamp}</span>"));

                // Individual messages
                foreach (var message in messageGroup.Messages)
                {
                    // Content
                    if (message.Content.IsNotBlank())
                    {
                        var content = FormatMessageContent(message.Content);
                        var contentHtml =
                            messageRightHtml.AppendChild(
                                HtmlNode.CreateNode($"<div class=\"msg-content\">{content}</div>"));

                        // Edited timestamp
                        if (message.EditedTimeStamp != null)
                        {
                            contentHtml.AppendChild(
                                HtmlNode.CreateNode(
                                    $"<span class=\"msg-edited\" title=\"{message.EditedTimeStamp:g}\">(edited)</span>"));
                        }
                    }

                    // Attachments
                    foreach (var attachment in message.Attachments)
                    {
                        if (attachment.Type == AttachmentType.Image)
                        {
                            messageRightHtml.AppendChild(
                                HtmlNode.CreateNode("<div class=\"msg-attachment\">" +
                                                    $"<a href=\"{attachment.Url}\">" +
                                                    $"<img class=\"msg-attachment\" src=\"{attachment.Url}\" />" +
                                                    "</a>" +
                                                    "</div>"));
                        }
                        else
                        {
                            messageRightHtml.AppendChild(
                                HtmlNode.CreateNode("<div class=\"msg-attachment\">" +
                                                    $"<a href=\"{attachment.Url}\">" +
                                                    $"Attachment: {attachment.FileName} ({NormalizeFileSize(attachment.FileSize)})" +
                                                    "</a>" +
                                                    "</div>"));
                        }
                    }
                }
            }

            doc.Save(filePath);
        }
    }

    public partial class ExportService
    {
        private static HtmlDocument GetTemplate()
        {
            var resourcePath = "DiscordChatExporter.Resources.ExportService.Template.html";

            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(resourcePath);
            if (stream == null)
                throw new MissingManifestResourceException("Could not find template resource");

            using (stream)
            {
                var doc = new HtmlDocument();
                doc.Load(stream);
                return doc;
            }
        }

        private static string GetStyle(Theme theme)
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

        private static string NormalizeFileSize(long fileSize)
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

        private static IEnumerable<MessageGroup> GroupMessages(IEnumerable<Message> messages)
        {
            var result = new List<MessageGroup>();

            // Group adjacent messages by timestamp and author
            var groupBuffer = new List<Message>();
            foreach (var message in messages)
            {
                var groupFirst = groupBuffer.FirstOrDefault();

                // Group break condition
                var breakCondition =
                    groupFirst != null &&
                    (
                        message.Author.Id != groupFirst.Author.Id ||
                        (message.TimeStamp - groupFirst.TimeStamp).TotalHours > 1 ||
                        message.TimeStamp.Hour != groupFirst.TimeStamp.Hour
                    );

                // If condition is true - flush buffer
                if (breakCondition)
                {
                    var group = new MessageGroup(groupFirst.Author, groupFirst.TimeStamp, groupBuffer);
                    result.Add(group);
                    groupBuffer.Clear();
                }

                // Add message to buffer
                groupBuffer.Add(message);
            }

            // Add what's remaining in buffer
            if (groupBuffer.Any())
            {
                var groupFirst = groupBuffer.First();
                var group = new MessageGroup(groupFirst.Author, groupFirst.TimeStamp, groupBuffer);
                result.Add(group);
            }

            return result;
        }

        private static string FormatMessageContent(string content)
        {
            // Encode HTML
            content = HtmlDocument.HtmlEncode(content);

            // Links from URLs
            content = Regex.Replace(content, "((https?|ftp)://[^\\s/$.?#].[^\\s]*)",
                "<a href=\"$1\">$1</a>");

            // Preformatted multiline
            content = Regex.Replace(content, "```([^`]*?)```",
                m => "<div class=\"pre\">" + m.Groups[1].Value + "</div>");

            // Preformatted inline
            content = Regex.Replace(content, "`([^`]*?)`",
                m => "<span class=\"pre\">" + m.Groups[1].Value + "</span>");

            // Bold
            content = Regex.Replace(content, "\\*\\*([^\\*]*?)\\*\\*", "<b>$1</b>");

            // Italic
            content = Regex.Replace(content, "\\*([^\\*]*?)\\*", "<i>$1</i>");

            // Underline
            content = Regex.Replace(content, "__([^_]*?)__", "<u>$1</u>");

            // Strike through
            content = Regex.Replace(content, "~~([^~]*?)~~", "<s>$1</s>");

            // New lines
            content = content.Replace("\n", "</br>");

            return content;
        }
    }
}
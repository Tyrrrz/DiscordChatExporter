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
    public class HtmlExportService
    {
        private HtmlDocument GetTemplate()
        {
            string templateName = "DiscordChatExporter.Resources.HtmlExportService.Template.html";

            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(templateName);
            if (stream == null)
                throw new MissingManifestResourceException("Could not find template resource");

            using (stream)
            {
                var doc = new HtmlDocument();
                doc.Load(stream);
                return doc;
            }
        }

        private string GetStyle(Theme theme)
        {
            string styleName = $"DiscordChatExporter.Resources.HtmlExportService.{theme}Theme.css";

            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(styleName);
            if (stream == null)
                throw new MissingManifestResourceException("Could not find theme style resource");

            using (stream)
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private IEnumerable<MessageGroup> GroupMessages(IEnumerable<Message> messages)
        {
            var result = new List<MessageGroup>();

            // Group adjacent messages by timestamp and author
            var groupBuffer = new List<Message>();
            foreach (var message in messages)
            {
                var groupFirst = groupBuffer.FirstOrDefault();

                // Group break condition
                bool breakCondition =
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

        private string FormatMessageContent(string content)
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

        public void Export(string filePath, Theme theme, ChatLog chatLog)
        {
            var doc = GetTemplate();
            string style = GetStyle(theme);

            // Set theme
            var themeHtml = doc.GetElementbyId("theme");
            themeHtml.InnerHtml = style;

            // Info
            var infoHtml = doc.GetElementbyId("info");
            infoHtml.AppendChild(HtmlNode.CreateNode($"<div>Channel ID: <b>{chatLog.ChannelId}</b></div>"));
            string participants = HtmlDocument.HtmlEncode(chatLog.Participants.Select(u => u.Name).JoinToString(", "));
            infoHtml.AppendChild(HtmlNode.CreateNode($"<div>Participants: <b>{participants}</b></div>"));
            infoHtml.AppendChild(HtmlNode.CreateNode($"<div>Messages: <b>{chatLog.Messages.Count:N0}</b></div>"));

            // Log
            var logHtml = doc.GetElementbyId("log");
            var messageGroups = GroupMessages(chatLog.Messages);
            foreach (var messageGroup in messageGroups)
            {
                // Container
                var messageHtml = logHtml.AppendChild(HtmlNode.CreateNode("<div class=\"msg\"></div>"));

                // Avatar
                messageHtml.AppendChild(HtmlNode.CreateNode("<div class=\"msg-avatar\">" +
                                                            $"<img class=\"msg-avatar\" src=\"{messageGroup.Author.AvatarUrl}\" />" +
                                                            "</div>"));

                // Body
                var messageBodyHtml = messageHtml.AppendChild(HtmlNode.CreateNode("<div class=\"msg-body\"></div>"));

                // Author
                string authorName = HtmlDocument.HtmlEncode(messageGroup.Author.Name);
                messageBodyHtml.AppendChild(HtmlNode.CreateNode($"<span class=\"msg-user\">{authorName}</span>"));

                // Date
                string timeStamp = HtmlDocument.HtmlEncode(messageGroup.FirstTimeStamp.ToString("g"));
                messageBodyHtml.AppendChild(HtmlNode.CreateNode($"<span class=\"msg-date\">{timeStamp}</span>"));

                // Individual messages
                foreach (var message in messageGroup.Messages)
                {
                    // Content
                    if (message.Content.IsNotBlank())
                    {
                        string content = FormatMessageContent(message.Content);
                        var contentHtml =
                            messageBodyHtml.AppendChild(
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
                        if (attachment.IsImage)
                        {
                            messageBodyHtml.AppendChild(
                                HtmlNode.CreateNode("<div class=\"msg-attachment\">" +
                                                    $"<a href=\"{attachment.Url}\">" +
                                                    $"<img class=\"msg-attachment\" src=\"{attachment.Url}\" />" +
                                                    "</a>" +
                                                    "</div>"));
                        }
                        else
                        {
                            messageBodyHtml.AppendChild(
                                HtmlNode.CreateNode("<div class=\"msg-attachment\">" +
                                                    $"<a href=\"{attachment.Url}\">" +
                                                    $"Attachment: {attachment.FileName}" +
                                                    "</a>" +
                                                    "</div>"));
                        }
                    }
                }
            }

            doc.Save(filePath);
        }
    }
}
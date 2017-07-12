using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DiscordChatExporter.Models;
using HtmlAgilityPack;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Services
{
    public class ExportService
    {
        private class MessageGroup
        {
            public User Author { get; }

            public DateTime FirstTimeStamp { get; }

            public IReadOnlyList<Message> Messages { get; }

            public MessageGroup(User author, DateTime firstTimeStamp, IEnumerable<Message> messages)
            {
                Author = author;
                FirstTimeStamp = firstTimeStamp;
                Messages = messages.ToArray();
            }
        }

        private HtmlDocument GetTemplate()
        {
            const string templateName = "DiscordChatExporter.Services.ExportTemplate.html";
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(templateName))
            {
                var doc = new HtmlDocument();
                doc.Load(stream);
                return doc;
            }
        }

        private IEnumerable<MessageGroup> GroupMessages(IEnumerable<Message> messages)
        {
            var result = new List<MessageGroup>();

            // Group adjacent messages by timestamp and author
            var buffer = new List<Message>();
            foreach (var message in messages)
            {
                var bufferFirst = buffer.FirstOrDefault();

                // Group break condition
                bool breakCondition =
                    bufferFirst != null &&
                    (
                        message.Author.Id != bufferFirst.Author.Id ||
                        (message.TimeStamp - bufferFirst.TimeStamp).TotalHours > 1 ||
                        message.TimeStamp.Hour != bufferFirst.TimeStamp.Hour
                    );

                // If condition is true - flush buffer
                if (breakCondition)
                {
                    var group = new MessageGroup(bufferFirst.Author, bufferFirst.TimeStamp, buffer);
                    result.Add(group);
                    buffer.Clear();
                }

                // Add message to buffer
                buffer.Add(message);
            }

            // Add what's remaining in buffer
            if (buffer.Any())
            {
                var bufferFirst = buffer.First();
                var group = new MessageGroup(bufferFirst.Author, bufferFirst.TimeStamp, buffer);
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
            content = Regex.Replace(content, "```([^`]*?)```", e => "<pre>" + e.Groups[1].Value + "</pre>");

            // Preformatted
            content = Regex.Replace(content, "`([^`]*?)`", e => "<pre>" + e.Groups[1].Value + "</pre>");

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

        public void Export(string filePath, ChatLog chatLog)
        {
            var doc = GetTemplate();

            // Info
            var infoHtml = doc.GetElementbyId("info");
            infoHtml.AppendChild(HtmlNode.CreateNode($"<div>Channel ID: <b>{chatLog.ChannelId}</b></div>"));
            string participants = HtmlDocument.HtmlEncode(chatLog.Participants.Select(u => u.Name).JoinToString(", "));
            infoHtml.AppendChild(HtmlNode.CreateNode($"<div>Participants: <b>{participants}</b></div>"));
            infoHtml.AppendChild(HtmlNode.CreateNode($"<div>Messages: <b>{chatLog.Messages.Count:N0}</b></div>"));

            // Messages
            var logHtml = doc.GetElementbyId("log");
            var messageGroups = GroupMessages(chatLog.Messages);
            foreach (var messageGroup in messageGroups)
            {
                // Container
                var messageHtml = logHtml.AppendChild(HtmlNode.CreateNode("<div class=\"msg\"></div>"));

                // Avatar
                messageHtml.AppendChild(HtmlNode.CreateNode("<img class=\"msg-avatar\" " +
                                                            $"src=\"{messageGroup.Author.AvatarUrl}\"></img>"));

                // Body
                var messageBodyHtml = messageHtml.AppendChild(HtmlNode.CreateNode("<div class=\"msg-body\"></div>"));

                // Author
                string authorName = HtmlDocument.HtmlEncode(messageGroup.Author.Name);
                messageBodyHtml.AppendChild(HtmlNode.CreateNode($"<span class=\"msg-user\">{authorName}</span>"));

                // Date
                string timeStamp = HtmlDocument.HtmlEncode(messageGroup.FirstTimeStamp.ToString("g"));
                messageBodyHtml.AppendChild(HtmlNode.CreateNode($"<span class=\"msg-date\">{timeStamp}</span>"));

                // Separate messages
                foreach (var message in messageGroup.Messages)
                {
                    // Content
                    if (message.Content.IsNotBlank())
                    {
                        string content = FormatMessageContent(message.Content);
                        var contentHtml = messageBodyHtml.AppendChild(HtmlNode.CreateNode($"<div class=\"msg-content\">{content}</div>"));

                        // Is edited
                        if (message.EditedTimeStamp != null)
                        {
                            contentHtml.AppendChild(
                                HtmlNode.CreateNode(
                                    $"<span class=\"msg-edited\" title=\"{message.EditedTimeStamp:g}\">(edited)</span>"));
                        }
                    }

                    // Attachments
                    if (message.Attachments.Any())
                    {
                        // Attachments
                        foreach (var attachment in message.Attachments)
                        {
                            if (attachment.IsImage)
                            {
                                messageBodyHtml.AppendChild(
                                    HtmlNode.CreateNode("<div class=\"msg-attachment\">" +
                                                        $"<a href=\"{attachment.Url}\">" +
                                                        $"<img class=\"msg-attachment\" src=\"{attachment.Url}\" />" +
                                                        "</a></div>"));
                            }
                            else
                            {
                                messageBodyHtml.AppendChild(
                                    HtmlNode.CreateNode("<div class=\"msg-attachment\">" +
                                                        $"<a href=\"{attachment.Url}\">" +
                                                        $"Attachment: {attachment.FileName}" +
                                                        "</a></div>"));
                            }
                        }
                    }
                }
            }

            doc.Save(filePath);
        }
    }
}
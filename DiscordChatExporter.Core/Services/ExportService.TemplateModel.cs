using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DiscordChatExporter.Core.Internal;
using DiscordChatExporter.Core.Markdown;
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
            private readonly ChatLog _log;
            private readonly string _dateFormat;
            private readonly int _messageGroupLimit;

            public TemplateModel(ExportFormat format, ChatLog log, string dateFormat, int messageGroupLimit)
            {
                _format = format;
                _log = log;
                _dateFormat = dateFormat;
                _messageGroupLimit = messageGroupLimit;
            }

            private IEnumerable<MessageGroup> GroupMessages(IEnumerable<Message> messages)
            {
                // Group adjacent messages by timestamp and author
                var buffer = new List<Message>();
                foreach (var message in messages)
                {
                    // Get first message of the group
                    var groupFirstMessage = buffer.FirstOrDefault();

                    // Group break condition
                    var breakCondition =
                        groupFirstMessage != null &&
                        (
                            message.Author.Id != groupFirstMessage.Author.Id || // when author changes
                            (message.Timestamp - groupFirstMessage.Timestamp).TotalHours > 1 || // when difference in timestamps is an hour or more
                            message.Timestamp.Hour != groupFirstMessage.Timestamp.Hour || // when the timestamp's hour changes
                            buffer.Count >= _messageGroupLimit // when group is full
                        );

                    // If condition is true - flush buffer
                    if (breakCondition)
                    {
                        var group = new MessageGroup(groupFirstMessage.Author, groupFirstMessage.Timestamp, buffer);

                        // Reset the buffer instead of clearing to avoid mutations on existing references
                        buffer = new List<Message>();

                        yield return group;
                    }

                    // Add message to buffer
                    buffer.Add(message);
                }

                // Add what's remaining in buffer
                if (buffer.Any())
                {
                    var groupFirstMessage = buffer.First();
                    var group = new MessageGroup(groupFirstMessage.Author, groupFirstMessage.Timestamp, buffer);

                    yield return group;
                }
            }

            private string Format(IFormattable obj, string format) =>
                obj.ToString(format, CultureInfo.InvariantCulture);

            private string FormatDate(DateTime dateTime) => Format(dateTime, _dateFormat);

            private string FormatMarkdownPlainText(IReadOnlyList<Node> nodes)
            {
                var buffer = new StringBuilder();

                foreach (var node in nodes)
                {
                    if (node is FormattedNode formattedNode)
                    {
                        var innerText = FormatMarkdownPlainText(formattedNode.Children);
                        buffer.Append($"{formattedNode.Token}{innerText}{formattedNode.Token}");
                    }

                    else if (node is MentionNode mentionNode && mentionNode.Type != MentionType.Meta)
                    {
                        if (mentionNode.Type == MentionType.User)
                        {
                            var user = _log.Mentionables.GetUser(mentionNode.Id);
                            buffer.Append($"@{user.Name}");
                        }

                        else if (mentionNode.Type == MentionType.Channel)
                        {
                            var channel = _log.Mentionables.GetChannel(mentionNode.Id);
                            buffer.Append($"#{channel.Name}");
                        }

                        else if (mentionNode.Type == MentionType.Role)
                        {
                            var role = _log.Mentionables.GetRole(mentionNode.Id);
                            buffer.Append($"@{role.Name}");
                        }
                    }

                    else if (node is EmojiNode emojiNode)
                    {
                        buffer.Append(emojiNode.IsCustomEmoji ? $":{emojiNode.Name}:" : node.Lexeme);
                    }

                    else
                    {
                        buffer.Append(node.Lexeme);
                    }
                }

                return buffer.ToString();
            }

            private string FormatMarkdownPlainText(string input)
                => FormatMarkdownPlainText(MarkdownParser.Parse(input));

            private string FormatMarkdownHtml(IReadOnlyList<Node> nodes, int depth = 0)
            {
                var buffer = new StringBuilder();

                foreach (var node in nodes)
                {
                    if (node is TextNode textNode)
                    {
                        buffer.Append(textNode.Text.HtmlEncode());
                    }

                    else if (node is FormattedNode formattedNode)
                    {
                        var innerHtml = FormatMarkdownHtml(formattedNode.Children, depth + 1);

                        if (formattedNode.Formatting == TextFormatting.Bold)
                            buffer.Append($"<strong>{innerHtml}</strong>");

                        else if (formattedNode.Formatting == TextFormatting.Italic)
                            buffer.Append($"<em>{innerHtml}</em>");

                        else if (formattedNode.Formatting == TextFormatting.Underline)
                            buffer.Append($"<u>{innerHtml}</u>");

                        else if (formattedNode.Formatting == TextFormatting.Strikethrough)
                            buffer.Append($"<s>{innerHtml}</s>");

                        else if (formattedNode.Formatting == TextFormatting.Spoiler)
                            buffer.Append($"<span class=\"spoiler\">{innerHtml}</span>");
                    }

                    else if (node is InlineCodeBlockNode inlineCodeBlockNode)
                    {
                        buffer.Append($"<span class=\"pre pre--inline\">{inlineCodeBlockNode.Code.HtmlEncode()}</span>");
                    }

                    else if (node is MultilineCodeBlockNode multilineCodeBlockNode)
                    {
                        // Set language class for syntax highlighting
                        var languageCssClass = multilineCodeBlockNode.Language.IsNotBlank()
                            ? "language-" + multilineCodeBlockNode.Language
                            : null;

                        buffer.Append(
                            $"<div class=\"pre pre--multiline {languageCssClass}\">{multilineCodeBlockNode.Code.HtmlEncode()}</div>");
                    }

                    else if (node is MentionNode mentionNode)
                    {
                        if (mentionNode.Type == MentionType.Meta)
                        {
                            buffer.Append($"<span class=\"mention\">@{mentionNode.Id.HtmlEncode()}</span>");
                        }

                        else if (mentionNode.Type == MentionType.User)
                        {
                            var user = _log.Mentionables.GetUser(mentionNode.Id);
                            buffer.Append($"<span class=\"mention\" title=\"{user.FullName}\">@{user.Name.HtmlEncode()}</span>");
                        }

                        else if (mentionNode.Type == MentionType.Channel)
                        {
                            var channel = _log.Mentionables.GetChannel(mentionNode.Id);
                            buffer.Append($"<span class=\"mention\">#{channel.Name.HtmlEncode()}</span>");
                        }

                        else if (mentionNode.Type == MentionType.Role)
                        {
                            var role = _log.Mentionables.GetRole(mentionNode.Id);
                            buffer.Append($"<span class=\"mention\">@{role.Name.HtmlEncode()}</span>");
                        }
                    }

                    else if (node is EmojiNode emojiNode)
                    {
                        // Get emoji image URL
                        var emojiImageUrl = new Emoji(emojiNode.Id, emojiNode.Name, emojiNode.IsAnimated).ImageUrl;

                        // Emoji can be jumboable if it's the only top-level node
                        var jumboableCssClass = depth == 0 && nodes.Count == 1
                            ? "emoji--large"
                            : null;

                        buffer.Append($"<img class=\"emoji {jumboableCssClass}\" title=\"{emojiNode.Name}\" src=\"{emojiImageUrl}\" />");
                    }

                    else if (node is LinkNode linkNode)
                    {
                        var escapedUrl = Uri.EscapeUriString(linkNode.Url);
                        buffer.Append($"<a href=\"{escapedUrl}\">{linkNode.Title.HtmlEncode()}</a>");
                    }
                }

                return buffer.ToString();
            }

            private string FormatMarkdownHtml(string input) 
                => FormatMarkdownHtml(MarkdownParser.Parse(input));

            private string FormatMarkdown(string input)
            {
                return _format == ExportFormat.HtmlDark || _format == ExportFormat.HtmlLight
                    ? FormatMarkdownHtml(input)
                    : FormatMarkdownPlainText(input);
            }

            public ScriptObject GetScriptObject()
            {
                // Create instance
                var scriptObject = new ScriptObject();

                // Import model
                scriptObject.SetValue("Model", _log, true);

                // Import functions
                scriptObject.Import(nameof(GroupMessages), new Func<IEnumerable<Message>, IEnumerable<MessageGroup>>(GroupMessages));
                scriptObject.Import(nameof(Format), new Func<IFormattable, string, string>(Format));
                scriptObject.Import(nameof(FormatDate), new Func<DateTime, string>(FormatDate));
                scriptObject.Import(nameof(FormatMarkdown), new Func<string, string>(FormatMarkdown));

                return scriptObject;
            }
        }
    }
}
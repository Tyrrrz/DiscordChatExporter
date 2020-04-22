using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Exporting.Writers.MarkdownVisitors;
using DiscordChatExporter.Domain.Internal;

namespace DiscordChatExporter.Domain.Exporting.Writers
{
    internal class PlainTextMessageWriter : MessageWriterBase
    {
        private readonly TextWriter _writer;

        private long _messageCount;

        public PlainTextMessageWriter(Stream stream, RenderContext context)
            : base(stream, context)
        {
            _writer = new StreamWriter(stream);
        }

        private string FormatMarkdown(string? markdown) =>
            PlainTextMarkdownVisitor.Format(Context, markdown ?? "");

        private string FormatMessageHeader(Message message)
        {
            var buffer = new StringBuilder();

            // Timestamp & author
            buffer
                .Append($"[{message.Timestamp.ToLocalString(Context.DateFormat)}]")
                .Append(' ')
                .Append($"{message.Author.FullName}");

            // Whether the message is pinned
            if (message.IsPinned)
                buffer.Append(' ').Append("(pinned)");

            return buffer.ToString();
        }

        private string FormatAttachments(IReadOnlyList<Attachment> attachments)
        {
            if (!attachments.Any())
                return "";

            var buffer = new StringBuilder();

            buffer
                .AppendLine("{Attachments}")
                .AppendJoin(Environment.NewLine, attachments.Select(a => a.Url))
                .AppendLine();

            return buffer.ToString();
        }

        private string FormatEmbeds(IReadOnlyList<Embed> embeds)
        {
            if (!embeds.Any())
                return "";

            var buffer = new StringBuilder();

            foreach (var embed in embeds)
            {
                buffer
                    .AppendLine("{Embed}")
                    .AppendLineIfNotNullOrWhiteSpace(embed.Author?.Name)
                    .AppendLineIfNotNullOrWhiteSpace(embed.Url)
                    .AppendLineIfNotNullOrWhiteSpace(FormatMarkdown(embed.Title))
                    .AppendLineIfNotNullOrWhiteSpace(FormatMarkdown(embed.Description));

                foreach (var field in embed.Fields)
                {
                    buffer
                        .AppendLineIfNotNullOrWhiteSpace(field.Name)
                        .AppendLineIfNotNullOrWhiteSpace(field.Value);
                }

                buffer
                    .AppendLineIfNotNullOrWhiteSpace(embed.Thumbnail?.Url)
                    .AppendLineIfNotNullOrWhiteSpace(embed.Image?.Url)
                    .AppendLineIfNotNullOrWhiteSpace(embed.Footer?.Text)
                    .AppendLine();
            }

            return buffer.ToString();
        }

        private string FormatReactions(IReadOnlyList<Reaction> reactions)
        {
            if (!reactions.Any())
                return "";

            var buffer = new StringBuilder();

            buffer.AppendLine("{Reactions}");

            foreach (var reaction in reactions)
            {
                buffer.Append(reaction.Emoji.Name);

                if (reaction.Count > 1)
                    buffer.Append($" ({reaction.Count})");

                buffer.Append(" ");
            }

            buffer.AppendLine();

            return buffer.ToString();
        }

        private string FormatMessage(Message message)
        {
            var buffer = new StringBuilder();

            buffer
                .AppendLine(FormatMessageHeader(message))
                .AppendLineIfNotNullOrWhiteSpace(FormatMarkdown(message.Content))
                .AppendLine()
                .AppendLineIfNotNullOrWhiteSpace(FormatAttachments(message.Attachments))
                .AppendLineIfNotNullOrWhiteSpace(FormatEmbeds(message.Embeds))
                .AppendLineIfNotNullOrWhiteSpace(FormatReactions(message.Reactions));

            return buffer.Trim().ToString();
        }

        public override async Task WritePreambleAsync()
        {
            var buffer = new StringBuilder();

            buffer.Append('=', 62).AppendLine();
            buffer.AppendLine($"Guild: {Context.Guild.Name}");
            buffer.AppendLine($"Channel: {Context.Channel.Name}");

            if (!string.IsNullOrWhiteSpace(Context.Channel.Topic))
                buffer.AppendLine($"Topic: {Context.Channel.Topic}");

            if (Context.After != null)
                buffer.AppendLine($"After: {Context.After.Value.ToLocalString(Context.DateFormat)}");

            if (Context.Before != null)
                buffer.AppendLine($"Before: {Context.Before.Value.ToLocalString(Context.DateFormat)}");

            buffer.Append('=', 62).AppendLine();

            await _writer.WriteLineAsync(buffer.ToString());
        }

        public override async Task WriteMessageAsync(Message message)
        {
            await _writer.WriteLineAsync(FormatMessage(message));
            await _writer.WriteLineAsync();

            _messageCount++;
        }

        public override async Task WritePostambleAsync()
        {
            var buffer = new StringBuilder();

            buffer
                .Append('=', 62).AppendLine()
                .AppendLine($"Exported {_messageCount:N0} message(s)")
                .Append('=', 62).AppendLine()
                .AppendLine();

            await _writer.WriteLineAsync(buffer.ToString());
        }

        public override async ValueTask DisposeAsync()
        {
            await _writer.DisposeAsync();
            await base.DisposeAsync();
        }
    }
}
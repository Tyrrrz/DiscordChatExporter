using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Exporting.Writers.MarkdownVisitors;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Domain.Exporting.Writers
{
    internal class PlainTextMessageWriter : MessageWriter
    {
        private readonly TextWriter _writer;

        private long _messageCount;

        public PlainTextMessageWriter(Stream stream, ExportContext context)
            : base(stream, context)
        {
            _writer = new StreamWriter(stream);
        }

        private string FormatMarkdown(string? markdown) =>
            PlainTextMarkdownVisitor.Format(Context, markdown ?? "");

        private async ValueTask WriteMessageHeaderAsync(Message message)
        {
            // Timestamp & author
            await _writer.WriteAsync($"[{Context.FormatDate(message.Timestamp)}]");
            await _writer.WriteAsync($" {message.Author.FullName}");

            // Whether the message is pinned
            if (message.IsPinned)
                await _writer.WriteAsync(" (pinned)");

            await _writer.WriteLineAsync();
        }

        private async ValueTask WriteAttachmentsAsync(IReadOnlyList<Attachment> attachments)
        {
            if (!attachments.Any())
                return;

            await _writer.WriteLineAsync("{Attachments}");

            foreach (var attachment in attachments)
                await _writer.WriteLineAsync(await Context.ResolveMediaUrlAsync(attachment.Url));

            await _writer.WriteLineAsync();
        }

        private async ValueTask WriteEmbedsAsync(IReadOnlyList<Embed> embeds)
        {
            foreach (var embed in embeds)
            {
                await _writer.WriteLineAsync("{Embed}");

                if (!string.IsNullOrWhiteSpace(embed.Author?.Name))
                    await _writer.WriteLineAsync(embed.Author.Name);

                if (!string.IsNullOrWhiteSpace(embed.Url))
                    await _writer.WriteLineAsync(embed.Url);

                if (!string.IsNullOrWhiteSpace(embed.Title))
                    await _writer.WriteLineAsync(FormatMarkdown(embed.Title));

                if (!string.IsNullOrWhiteSpace(embed.Description))
                    await _writer.WriteLineAsync(FormatMarkdown(embed.Description));

                foreach (var field in embed.Fields)
                {
                    if (!string.IsNullOrWhiteSpace(field.Name))
                        await _writer.WriteLineAsync(FormatMarkdown(field.Name));

                    if (!string.IsNullOrWhiteSpace(field.Value))
                        await _writer.WriteLineAsync(FormatMarkdown(field.Value));
                }

                if (!string.IsNullOrWhiteSpace(embed.Thumbnail?.Url))
                    await _writer.WriteLineAsync(await Context.ResolveMediaUrlAsync(embed.Thumbnail.Url));

                if (!string.IsNullOrWhiteSpace(embed.Image?.Url))
                    await _writer.WriteLineAsync(await Context.ResolveMediaUrlAsync(embed.Image.Url));

                if (!string.IsNullOrWhiteSpace(embed.Footer?.Text))
                    await _writer.WriteLineAsync(embed.Footer.Text);

                await _writer.WriteLineAsync();
            }
        }

        private async ValueTask WriteReactionsAsync(IReadOnlyList<Reaction> reactions)
        {
            if (!reactions.Any())
                return;

            await _writer.WriteLineAsync("{Reactions}");

            foreach (var reaction in reactions)
            {
                await _writer.WriteAsync(reaction.Emoji.Name);

                if (reaction.Count > 1)
                    await _writer.WriteAsync($" ({reaction.Count})");

                await _writer.WriteAsync(' ');
            }

            await _writer.WriteLineAsync();
        }

        public override async ValueTask WritePreambleAsync()
        {
            await _writer.WriteLineAsync('='.Repeat(62));
            await _writer.WriteLineAsync($"Guild: {Context.Request.Guild.Name}");
            await _writer.WriteLineAsync($"Channel: {Context.Request.Channel.Category.Name} / {Context.Request.Channel.Name}");

            if (!string.IsNullOrWhiteSpace(Context.Request.Channel.Topic))
                await _writer.WriteLineAsync($"Topic: {Context.Request.Channel.Topic}");

            if (Context.Request.After != null)
                await _writer.WriteLineAsync($"After: {Context.FormatDate(Context.Request.After.Value.ToDate())}");

            if (Context.Request.Before != null)
                await _writer.WriteLineAsync($"Before: {Context.FormatDate(Context.Request.Before.Value.ToDate())}");

            await _writer.WriteLineAsync('='.Repeat(62));
            await _writer.WriteLineAsync();
        }

        public override async ValueTask WriteMessageAsync(Message message)
        {
            await WriteMessageHeaderAsync(message);

            if (!string.IsNullOrWhiteSpace(message.Content))
                await _writer.WriteLineAsync(FormatMarkdown(message.Content));

            await _writer.WriteLineAsync();

            await WriteAttachmentsAsync(message.Attachments);
            await WriteEmbedsAsync(message.Embeds);
            await WriteReactionsAsync(message.Reactions);

            await _writer.WriteLineAsync();

            _messageCount++;
        }

        public override async ValueTask WritePostambleAsync()
        {
            await _writer.WriteLineAsync('='.Repeat(62));
            await _writer.WriteLineAsync($"Exported {_messageCount:N0} message(s)");
            await _writer.WriteLineAsync('='.Repeat(62));
        }

        public override async ValueTask DisposeAsync()
        {
            await _writer.DisposeAsync();
            await base.DisposeAsync();
        }
    }
}
﻿using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Exporting.Writers.MarkdownVisitors;
using DiscordChatExporter.Domain.Internal.Extensions;

namespace DiscordChatExporter.Domain.Exporting.Writers
{
    internal class JsonMessageWriter : MessageWriter
    {
        private readonly Utf8JsonWriter _writer;

        private long _messageCount;

        public JsonMessageWriter(Stream stream, ExportContext context)
            : base(stream, context)
        {
            _writer = new Utf8JsonWriter(stream, new JsonWriterOptions
            {
                Indented = true
            });
        }

        private string FormatMarkdown(string? markdown) =>
            PlainTextMarkdownVisitor.Format(Context, markdown ?? "");

        private async ValueTask WriteAttachmentAsync(Attachment attachment)
        {
            _writer.WriteStartObject();

            _writer.WriteString("id", attachment.Id);
            _writer.WriteString("url", await Context.ResolveMediaUrlAsync(attachment.Url));
            _writer.WriteString("fileName", attachment.FileName);
            _writer.WriteNumber("fileSizeBytes", attachment.FileSize.TotalBytes);

            _writer.WriteEndObject();
            await _writer.FlushAsync();
        }

        private async ValueTask WriteEmbedAuthorAsync(EmbedAuthor embedAuthor)
        {
            _writer.WriteStartObject("author");

            _writer.WriteString("name", embedAuthor.Name);
            _writer.WriteString("url", embedAuthor.Url);

            if (!string.IsNullOrWhiteSpace(embedAuthor.IconUrl))
                _writer.WriteString("iconUrl", await Context.ResolveMediaUrlAsync(embedAuthor.IconUrl));

            _writer.WriteEndObject();
            await _writer.FlushAsync();
        }

        private async ValueTask WriteEmbedThumbnailAsync(EmbedImage embedThumbnail)
        {
            _writer.WriteStartObject("thumbnail");

            if (!string.IsNullOrWhiteSpace(embedThumbnail.Url))
                _writer.WriteString("url", await Context.ResolveMediaUrlAsync(embedThumbnail.Url));

            _writer.WriteNumber("width", embedThumbnail.Width);
            _writer.WriteNumber("height", embedThumbnail.Height);

            _writer.WriteEndObject();
            await _writer.FlushAsync();
        }

        private async ValueTask WriteEmbedImageAsync(EmbedImage embedImage)
        {
            _writer.WriteStartObject("image");

            if (!string.IsNullOrWhiteSpace(embedImage.Url))
                _writer.WriteString("url", await Context.ResolveMediaUrlAsync(embedImage.Url));

            _writer.WriteNumber("width", embedImage.Width);
            _writer.WriteNumber("height", embedImage.Height);

            _writer.WriteEndObject();
            await _writer.FlushAsync();
        }

        private async ValueTask WriteEmbedFooterAsync(EmbedFooter embedFooter)
        {
            _writer.WriteStartObject("footer");

            _writer.WriteString("text", embedFooter.Text);

            if (!string.IsNullOrWhiteSpace(embedFooter.IconUrl))
                _writer.WriteString("iconUrl", await Context.ResolveMediaUrlAsync(embedFooter.IconUrl));

            _writer.WriteEndObject();
            await _writer.FlushAsync();
        }

        private async ValueTask WriteEmbedFieldAsync(EmbedField embedField)
        {
            _writer.WriteStartObject();

            _writer.WriteString("name", FormatMarkdown(embedField.Name));
            _writer.WriteString("value", FormatMarkdown(embedField.Value));
            _writer.WriteBoolean("isInline", embedField.IsInline);

            _writer.WriteEndObject();
            await _writer.FlushAsync();
        }

        private async ValueTask WriteEmbedAsync(Embed embed)
        {
            _writer.WriteStartObject();

            _writer.WriteString("title", FormatMarkdown(embed.Title));
            _writer.WriteString("url", embed.Url);
            _writer.WriteString("timestamp", embed.Timestamp);
            _writer.WriteString("description", FormatMarkdown(embed.Description));

            if (embed.Color != null)
                _writer.WriteString("color", embed.Color.Value.ToHex());

            if (embed.Author != null)
                await WriteEmbedAuthorAsync(embed.Author);

            if (embed.Thumbnail != null)
                await WriteEmbedThumbnailAsync(embed.Thumbnail);

            if (embed.Image != null)
                await WriteEmbedImageAsync(embed.Image);

            if (embed.Footer != null)
                await WriteEmbedFooterAsync(embed.Footer);

            // Fields
            _writer.WriteStartArray("fields");

            foreach (var field in embed.Fields)
                await WriteEmbedFieldAsync(field);

            _writer.WriteEndArray();

            _writer.WriteEndObject();
            await _writer.FlushAsync();
        }

        private async ValueTask WriteReactionAsync(Reaction reaction)
        {
            _writer.WriteStartObject();

            // Emoji
            _writer.WriteStartObject("emoji");
            _writer.WriteString("id", reaction.Emoji.Id);
            _writer.WriteString("name", reaction.Emoji.Name);
            _writer.WriteBoolean("isAnimated", reaction.Emoji.IsAnimated);
            _writer.WriteString("imageUrl", await Context.ResolveMediaUrlAsync(reaction.Emoji.ImageUrl));
            _writer.WriteEndObject();

            _writer.WriteNumber("count", reaction.Count);

            _writer.WriteEndObject();
            await _writer.FlushAsync();
        }

        public override async ValueTask WritePreambleAsync()
        {
            // Root object (start)
            _writer.WriteStartObject();

            // Guild
            _writer.WriteStartObject("guild");
            _writer.WriteString("id", Context.Request.Guild.Id);
            _writer.WriteString("name", Context.Request.Guild.Name);
            _writer.WriteString("iconUrl", await Context.ResolveMediaUrlAsync(Context.Request.Guild.IconUrl));
            _writer.WriteEndObject();

            // Channel
            _writer.WriteStartObject("channel");
            _writer.WriteString("id", Context.Request.Channel.Id);
            _writer.WriteString("type", Context.Request.Channel.Type.ToString());
            _writer.WriteString("category", Context.Request.Channel.Category);
            _writer.WriteString("name", Context.Request.Channel.Name);
            _writer.WriteString("topic", Context.Request.Channel.Topic);
            _writer.WriteEndObject();

            // Date range
            _writer.WriteStartObject("dateRange");
            _writer.WriteString("after", Context.Request.After);
            _writer.WriteString("before", Context.Request.Before);
            _writer.WriteEndObject();

            // Message array (start)
            _writer.WriteStartArray("messages");
            await _writer.FlushAsync();
        }

        public override async ValueTask WriteMessageAsync(Message message)
        {
            _writer.WriteStartObject();

            // Metadata
            _writer.WriteString("id", message.Id);
            _writer.WriteString("type", message.Type.ToString());
            _writer.WriteString("timestamp", message.Timestamp);
            _writer.WriteString("timestampEdited", message.EditedTimestamp);
            _writer.WriteString("callEndedTimestamp", message.CallEndedTimestamp);
            _writer.WriteBoolean("isPinned", message.IsPinned);

            // Content
            _writer.WriteString("content", FormatMarkdown(message.Content));

            // Author
            _writer.WriteStartObject("author");
            _writer.WriteString("id", message.Author.Id);
            _writer.WriteString("name", message.Author.Name);
            _writer.WriteString("discriminator", $"{message.Author.Discriminator:0000}");
            _writer.WriteBoolean("isBot", message.Author.IsBot);
            _writer.WriteString("avatarUrl", await Context.ResolveMediaUrlAsync(message.Author.AvatarUrl));
            _writer.WriteEndObject();

            // Attachments
            _writer.WriteStartArray("attachments");

            foreach (var attachment in message.Attachments)
                await WriteAttachmentAsync(attachment);

            _writer.WriteEndArray();

            // Embeds
            _writer.WriteStartArray("embeds");

            foreach (var embed in message.Embeds)
                await WriteEmbedAsync(embed);

            _writer.WriteEndArray();

            // Reactions
            _writer.WriteStartArray("reactions");

            foreach (var reaction in message.Reactions)
                await WriteReactionAsync(reaction);

            _writer.WriteEndArray();

            _writer.WriteEndObject();
            await _writer.FlushAsync();

            _messageCount++;
        }

        public override async ValueTask WritePostambleAsync()
        {
            // Message array (end)
            _writer.WriteEndArray();

            _writer.WriteNumber("messageCount", _messageCount);

            // Root object (end)
            _writer.WriteEndObject();
            await _writer.FlushAsync();
        }

        public override async ValueTask DisposeAsync()
        {
            await _writer.DisposeAsync();
            await base.DisposeAsync();
        }
    }
}
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Exporting.Writers.MarkdownVisitors;
using DiscordChatExporter.Domain.Internal;

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

        private void WriteAttachment(Attachment attachment)
        {
            _writer.WriteStartObject();

            _writer.WriteString("id", attachment.Id);
            _writer.WriteString("url", attachment.Url);
            _writer.WriteString("fileName", attachment.FileName);
            _writer.WriteNumber("fileSizeBytes", attachment.FileSize.TotalBytes);

            _writer.WriteEndObject();
        }

        private void WriteEmbedAuthor(EmbedAuthor embedAuthor)
        {
            _writer.WriteStartObject("author");

            _writer.WriteString("name", embedAuthor.Name);
            _writer.WriteString("url", embedAuthor.Url);
            _writer.WriteString("iconUrl", embedAuthor.IconUrl);

            _writer.WriteEndObject();
        }

        private void WriteEmbedThumbnail(EmbedImage embedThumbnail)
        {
            _writer.WriteStartObject("thumbnail");

            _writer.WriteString("url", embedThumbnail.Url);
            _writer.WriteNumber("width", embedThumbnail.Width);
            _writer.WriteNumber("height", embedThumbnail.Height);

            _writer.WriteEndObject();
        }

        private void WriteEmbedImage(EmbedImage embedImage)
        {
            _writer.WriteStartObject("image");

            _writer.WriteString("url", embedImage.Url);
            _writer.WriteNumber("width", embedImage.Width);
            _writer.WriteNumber("height", embedImage.Height);

            _writer.WriteEndObject();
        }

        private void WriteEmbedFooter(EmbedFooter embedFooter)
        {
            _writer.WriteStartObject("footer");

            _writer.WriteString("text", embedFooter.Text);
            _writer.WriteString("iconUrl", embedFooter.IconUrl);

            _writer.WriteEndObject();
        }

        private void WriteEmbedField(EmbedField embedField)
        {
            _writer.WriteStartObject();

            _writer.WriteString("name", embedField.Name);
            _writer.WriteString("value", embedField.Value);
            _writer.WriteBoolean("isInline", embedField.IsInline);

            _writer.WriteEndObject();
        }

        private void WriteEmbed(Embed embed)
        {
            _writer.WriteStartObject();

            _writer.WriteString("title", FormatMarkdown(embed.Title));
            _writer.WriteString("url", embed.Url);
            _writer.WriteString("timestamp", embed.Timestamp);
            _writer.WriteString("description", FormatMarkdown(embed.Description));

            if (embed.Author != null)
                WriteEmbedAuthor(embed.Author);

            if (embed.Thumbnail != null)
                WriteEmbedThumbnail(embed.Thumbnail);

            if (embed.Image != null)
                WriteEmbedImage(embed.Image);

            if (embed.Footer != null)
                WriteEmbedFooter(embed.Footer);

            // Fields
            _writer.WriteStartArray("fields");

            foreach (var field in embed.Fields)
                WriteEmbedField(field);

            _writer.WriteEndArray();

            _writer.WriteEndObject();
        }

        private void WriteReaction(Reaction reaction)
        {
            _writer.WriteStartObject();

            // Emoji
            _writer.WriteStartObject("emoji");
            _writer.WriteString("id", reaction.Emoji.Id);
            _writer.WriteString("name", reaction.Emoji.Name);
            _writer.WriteBoolean("isAnimated", reaction.Emoji.IsAnimated);
            _writer.WriteString("imageUrl", reaction.Emoji.ImageUrl);
            _writer.WriteEndObject();

            _writer.WriteNumber("count", reaction.Count);

            _writer.WriteEndObject();
        }

        public override async Task WritePreambleAsync()
        {
            // Root object (start)
            _writer.WriteStartObject();

            // Guild
            _writer.WriteStartObject("guild");
            _writer.WriteString("id", Context.Guild.Id);
            _writer.WriteString("name", Context.Guild.Name);
            _writer.WriteString("iconUrl", Context.Guild.IconUrl);
            _writer.WriteEndObject();

            // Channel
            _writer.WriteStartObject("channel");
            _writer.WriteString("id", Context.Channel.Id);
            _writer.WriteString("type", Context.Channel.Type.ToString());
            _writer.WriteString("name", Context.Channel.Name);
            _writer.WriteString("topic", Context.Channel.Topic);
            _writer.WriteEndObject();

            // Date range
            _writer.WriteStartObject("dateRange");
            _writer.WriteString("after", Context.After);
            _writer.WriteString("before", Context.Before);
            _writer.WriteEndObject();

            // Message array (start)
            _writer.WriteStartArray("messages");

            await _writer.FlushAsync();
        }

        public override async Task WriteMessageAsync(Message message)
        {
            _writer.WriteStartObject();

            // Metadata
            _writer.WriteString("id", message.Id);
            _writer.WriteString("type", message.Type.ToString());
            _writer.WriteString("timestamp", message.Timestamp);
            _writer.WriteString("timestampEdited", message.EditedTimestamp);
            _writer.WriteBoolean("isPinned", message.IsPinned);

            // Content
            _writer.WriteString("content", FormatMarkdown(message.Content));

            // Author
            _writer.WriteStartObject("author");
            _writer.WriteString("id", message.Author.Id);
            _writer.WriteString("name", message.Author.Name);
            _writer.WriteString("discriminator", $"{message.Author.Discriminator:0000}");
            _writer.WriteBoolean("isBot", message.Author.IsBot);
            _writer.WriteString("avatarUrl", message.Author.AvatarUrl);
            _writer.WriteEndObject();

            // Attachments
            _writer.WriteStartArray("attachments");

            foreach (var attachment in message.Attachments)
                WriteAttachment(attachment);

            _writer.WriteEndArray();

            // Embeds
            _writer.WriteStartArray("embeds");

            foreach (var embed in message.Embeds)
                WriteEmbed(embed);

            _writer.WriteEndArray();

            // Reactions
            _writer.WriteStartArray("reactions");

            foreach (var reaction in message.Reactions)
                WriteReaction(reaction);

            _writer.WriteEndArray();

            _writer.WriteEndObject();

            // Flush every 100 messages
            if (_messageCount++ % 100 == 0)
                await _writer.FlushAsync();
        }

        public override async Task WritePostambleAsync()
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
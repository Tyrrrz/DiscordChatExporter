﻿using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Discord.Data.Embeds;
using DiscordChatExporter.Core.Exporting.Writers.MarkdownVisitors;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Writing;

namespace DiscordChatExporter.Core.Exporting.Writers;

internal class JsonMessageWriter : MessageWriter
{
    private readonly Utf8JsonWriter _writer;

    public JsonMessageWriter(Stream stream, ExportContext context)
        : base(stream, context)
    {
        _writer = new Utf8JsonWriter(stream, new JsonWriterOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Indented = true,
            // Validation errors may mask actual failures
            // https://github.com/Tyrrrz/DiscordChatExporter/issues/413
            SkipValidation = true
        });
    }

    private string FormatMarkdown(string? markdown) =>
        PlainTextMarkdownVisitor.Format(Context, markdown ?? "");

    private async ValueTask WriteAttachmentAsync(
        Attachment attachment,
        CancellationToken cancellationToken = default)
    {
        _writer.WriteStartObject();

        _writer.WriteString("id", attachment.Id.ToString());
        _writer.WriteString("url", await Context.ResolveMediaUrlAsync(attachment.Url, cancellationToken));
        _writer.WriteString("fileName", attachment.FileName);
        _writer.WriteNumber("fileSizeBytes", attachment.FileSize.TotalBytes);

        _writer.WriteEndObject();
        await _writer.FlushAsync(cancellationToken);
    }

    private async ValueTask WriteEmbedAuthorAsync(
        EmbedAuthor embedAuthor,
        CancellationToken cancellationToken = default)
    {
        _writer.WriteStartObject("author");

        _writer.WriteString("name", embedAuthor.Name);
        _writer.WriteString("url", embedAuthor.Url);

        if (!string.IsNullOrWhiteSpace(embedAuthor.IconUrl))
            _writer.WriteString("iconUrl", await Context.ResolveMediaUrlAsync(embedAuthor.IconProxyUrl ?? embedAuthor.IconUrl, cancellationToken));

        _writer.WriteEndObject();
        await _writer.FlushAsync(cancellationToken);
    }

    private async ValueTask WriteEmbedThumbnailAsync(
        EmbedImage embedThumbnail,
        CancellationToken cancellationToken = default)
    {
        _writer.WriteStartObject("thumbnail");

        if (!string.IsNullOrWhiteSpace(embedThumbnail.Url))
            _writer.WriteString("url", await Context.ResolveMediaUrlAsync(embedThumbnail.ProxyUrl ?? embedThumbnail.Url, cancellationToken));

        _writer.WriteNumber("width", embedThumbnail.Width);
        _writer.WriteNumber("height", embedThumbnail.Height);

        _writer.WriteEndObject();
        await _writer.FlushAsync(cancellationToken);
    }

    private async ValueTask WriteEmbedImageAsync(
        EmbedImage embedImage,
        CancellationToken cancellationToken = default)
    {
        _writer.WriteStartObject("image");

        if (!string.IsNullOrWhiteSpace(embedImage.Url))
            _writer.WriteString("url", await Context.ResolveMediaUrlAsync(embedImage.ProxyUrl ?? embedImage.Url, cancellationToken));

        _writer.WriteNumber("width", embedImage.Width);
        _writer.WriteNumber("height", embedImage.Height);

        _writer.WriteEndObject();
        await _writer.FlushAsync(cancellationToken);
    }

    private async ValueTask WriteEmbedFooterAsync(
        EmbedFooter embedFooter,
        CancellationToken cancellationToken = default)
    {
        _writer.WriteStartObject("footer");

        _writer.WriteString("text", embedFooter.Text);

        if (!string.IsNullOrWhiteSpace(embedFooter.IconUrl))
            _writer.WriteString("iconUrl", await Context.ResolveMediaUrlAsync(embedFooter.IconProxyUrl ?? embedFooter.IconUrl, cancellationToken));

        _writer.WriteEndObject();
        await _writer.FlushAsync(cancellationToken);
    }

    private async ValueTask WriteEmbedFieldAsync(
        EmbedField embedField,
        CancellationToken cancellationToken = default)
    {
        _writer.WriteStartObject();

        _writer.WriteString("name", FormatMarkdown(embedField.Name));
        _writer.WriteString("value", FormatMarkdown(embedField.Value));
        _writer.WriteBoolean("isInline", embedField.IsInline);

        _writer.WriteEndObject();
        await _writer.FlushAsync(cancellationToken);
    }

    private async ValueTask WriteEmbedAsync(
        Embed embed,
        CancellationToken cancellationToken = default)
    {
        _writer.WriteStartObject();

        _writer.WriteString("title", FormatMarkdown(embed.Title));
        _writer.WriteString("url", embed.Url);
        _writer.WriteString("timestamp", embed.Timestamp);
        _writer.WriteString("description", FormatMarkdown(embed.Description));

        if (embed.Color is not null)
            _writer.WriteString("color", embed.Color.Value.ToHex());

        if (embed.Author is not null)
            await WriteEmbedAuthorAsync(embed.Author, cancellationToken);

        if (embed.Thumbnail is not null)
            await WriteEmbedThumbnailAsync(embed.Thumbnail, cancellationToken);

        if (embed.Image is not null)
            await WriteEmbedImageAsync(embed.Image, cancellationToken);

        if (embed.Footer is not null)
            await WriteEmbedFooterAsync(embed.Footer, cancellationToken);

        // Fields
        _writer.WriteStartArray("fields");

        foreach (var field in embed.Fields)
            await WriteEmbedFieldAsync(field, cancellationToken);

        _writer.WriteEndArray();

        _writer.WriteEndObject();
        await _writer.FlushAsync(cancellationToken);
    }

    private async ValueTask WriteStickerAsync(
        Sticker sticker,
        CancellationToken cancellationToken = default)
    {
        _writer.WriteStartObject();

        _writer.WriteString("id", sticker.Id.ToString());
        _writer.WriteString("name", sticker.Name);
        _writer.WriteString("format", sticker.Format.ToString());
        _writer.WriteString("sourceUrl", await Context.ResolveMediaUrlAsync(sticker.SourceUrl, cancellationToken));

        _writer.WriteEndObject();
        await _writer.FlushAsync(cancellationToken);
    }

    private async ValueTask WriteReactionAsync(
        Reaction reaction,
        CancellationToken cancellationToken = default)
    {
        _writer.WriteStartObject();

        // Emoji
        _writer.WriteStartObject("emoji");
        _writer.WriteString("id", reaction.Emoji.Id.ToString());
        _writer.WriteString("name", reaction.Emoji.Name);
        _writer.WriteBoolean("isAnimated", reaction.Emoji.IsAnimated);
        _writer.WriteString("imageUrl", await Context.ResolveMediaUrlAsync(reaction.Emoji.ImageUrl, cancellationToken));
        _writer.WriteEndObject();

        _writer.WriteNumber("count", reaction.Count);

        _writer.WriteEndObject();
        await _writer.FlushAsync(cancellationToken);
    }

    private async ValueTask WriteMentionAsync(
        User mentionedUser,
        CancellationToken cancellationToken = default)
    {
        _writer.WriteStartObject();

        _writer.WriteString("id", mentionedUser.Id.ToString());
        _writer.WriteString("name", mentionedUser.Name);
        _writer.WriteString("discriminator", mentionedUser.DiscriminatorFormatted);
        _writer.WriteString("nickname", Context.TryGetMember(mentionedUser.Id)?.Nick ?? mentionedUser.Name);
        _writer.WriteBoolean("isBot", mentionedUser.IsBot);

        _writer.WriteEndObject();
        await _writer.FlushAsync(cancellationToken);
    }

    public override async ValueTask WritePreambleAsync(CancellationToken cancellationToken = default)
    {
        // Root object (start)
        _writer.WriteStartObject();

        // Guild
        _writer.WriteStartObject("guild");
        _writer.WriteString("id", Context.Request.Guild.Id.ToString());
        _writer.WriteString("name", Context.Request.Guild.Name);
        _writer.WriteString("iconUrl", await Context.ResolveMediaUrlAsync(Context.Request.Guild.IconUrl, cancellationToken));
        _writer.WriteEndObject();

        // Channel
        _writer.WriteStartObject("channel");
        _writer.WriteString("id", Context.Request.Channel.Id.ToString());
        _writer.WriteString("type", Context.Request.Channel.Kind.ToString());
        _writer.WriteString("categoryId", Context.Request.Channel.Category.Id.ToString());
        _writer.WriteString("category", Context.Request.Channel.Category.Name);
        _writer.WriteString("name", Context.Request.Channel.Name);
        _writer.WriteString("topic", Context.Request.Channel.Topic);
        _writer.WriteEndObject();

        // Date range
        _writer.WriteStartObject("dateRange");
        _writer.WriteString("after", Context.Request.After?.ToDate());
        _writer.WriteString("before", Context.Request.Before?.ToDate());
        _writer.WriteEndObject();

        // Message array (start)
        _writer.WriteStartArray("messages");
        await _writer.FlushAsync(cancellationToken);
    }

    public override async ValueTask WriteMessageAsync(
        Message message,
        CancellationToken cancellationToken = default)
    {
        await base.WriteMessageAsync(message, cancellationToken);

        _writer.WriteStartObject();

        // Metadata
        _writer.WriteString("id", message.Id.ToString());
        _writer.WriteString("type", message.Kind.ToString());
        _writer.WriteString("timestamp", message.Timestamp);
        _writer.WriteString("timestampEdited", message.EditedTimestamp);
        _writer.WriteString("callEndedTimestamp", message.CallEndedTimestamp);
        _writer.WriteBoolean("isPinned", message.IsPinned);

        // Content
        _writer.WriteString("content", FormatMarkdown(message.Content));

        // Author
        _writer.WriteStartObject("author");
        _writer.WriteString("id", message.Author.Id.ToString());
        _writer.WriteString("name", message.Author.Name);
        _writer.WriteString("discriminator", message.Author.DiscriminatorFormatted);
        _writer.WriteString("nickname", Context.TryGetMember(message.Author.Id)?.Nick ?? message.Author.Name);
        _writer.WriteString("color", Context.TryGetUserColor(message.Author.Id)?.ToHex());
        _writer.WriteBoolean("isBot", message.Author.IsBot);
        _writer.WriteString("avatarUrl", await Context.ResolveMediaUrlAsync(message.Author.AvatarUrl, cancellationToken));
        _writer.WriteEndObject();

        // Attachments
        _writer.WriteStartArray("attachments");

        foreach (var attachment in message.Attachments)
            await WriteAttachmentAsync(attachment, cancellationToken);

        _writer.WriteEndArray();

        // Embeds
        _writer.WriteStartArray("embeds");

        foreach (var embed in message.Embeds)
            await WriteEmbedAsync(embed, cancellationToken);

        _writer.WriteEndArray();

        // Stickers
        _writer.WriteStartArray("stickers");

        foreach (var sticker in message.Stickers)
            await WriteStickerAsync(sticker, cancellationToken);

        _writer.WriteEndArray();

        // Reactions
        _writer.WriteStartArray("reactions");

        foreach (var reaction in message.Reactions)
            await WriteReactionAsync(reaction, cancellationToken);

        _writer.WriteEndArray();

        // Mentions
        _writer.WriteStartArray("mentions");

        foreach (var mention in message.MentionedUsers)
            await WriteMentionAsync(mention, cancellationToken);

        _writer.WriteEndArray();

        // Message reference
        if (message.Reference is not null)
        {
            _writer.WriteStartObject("reference");
            _writer.WriteString("messageId", message.Reference.MessageId?.ToString());
            _writer.WriteString("channelId", message.Reference.ChannelId?.ToString());
            _writer.WriteString("guildId", message.Reference.GuildId?.ToString());
            _writer.WriteEndObject();
        }

        _writer.WriteEndObject();
        await _writer.FlushAsync(cancellationToken);
    }

    public override async ValueTask WritePostambleAsync(CancellationToken cancellationToken = default)
    {
        // Message array (end)
        _writer.WriteEndArray();

        _writer.WriteNumber("messageCount", MessagesWritten);

        // Root object (end)
        _writer.WriteEndObject();
        await _writer.FlushAsync(cancellationToken);
    }

    public override async ValueTask DisposeAsync()
    {
        await _writer.DisposeAsync();
        await base.DisposeAsync();
    }
}
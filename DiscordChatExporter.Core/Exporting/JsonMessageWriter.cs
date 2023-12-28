using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Discord.Data.Embeds;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Writing;

namespace DiscordChatExporter.Core.Exporting;

internal class JsonMessageWriter(Stream stream, ExportContext context)
    : MessageWriter(stream, context)
{
    private readonly Utf8JsonWriter _writer =
        new(
            stream,
            new JsonWriterOptions
            {
                // https://github.com/Tyrrrz/DiscordChatExporter/issues/450
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Indented = true,
                // Validation errors may mask actual failures
                // https://github.com/Tyrrrz/DiscordChatExporter/issues/413
                SkipValidation = true
            }
        );

    private async ValueTask<string> FormatMarkdownAsync(
        string markdown,
        CancellationToken cancellationToken = default
    ) =>
        Context.Request.ShouldFormatMarkdown
            ? await PlainTextMarkdownVisitor.FormatAsync(Context, markdown, cancellationToken)
            : markdown;

    private async ValueTask WriteUserAsync(User user, CancellationToken cancellationToken = default)
    {
        _writer.WriteStartObject();

        _writer.WriteString("id", user.Id.ToString());
        _writer.WriteString("name", user.Name);
        _writer.WriteString("discriminator", user.DiscriminatorFormatted);
        _writer.WriteString(
            "nickname",
            Context.TryGetMember(user.Id)?.DisplayName ?? user.DisplayName
        );
        _writer.WriteString("color", Context.TryGetUserColor(user.Id)?.ToHex());
        _writer.WriteBoolean("isBot", user.IsBot);

        _writer.WritePropertyName("roles");
        await WriteRolesAsync(Context.GetUserRoles(user.Id), cancellationToken);

        _writer.WriteString(
            "avatarUrl",
            await Context.ResolveAssetUrlAsync(
                Context.TryGetMember(user.Id)?.AvatarUrl ?? user.AvatarUrl,
                cancellationToken
            )
        );

        _writer.WriteEndObject();
        await _writer.FlushAsync(cancellationToken);
    }

    private async ValueTask WriteRolesAsync(
        IReadOnlyList<Role> roles,
        CancellationToken cancellationToken = default
    )
    {
        _writer.WriteStartArray();

        foreach (var role in roles)
        {
            _writer.WriteStartObject();

            _writer.WriteString("id", role.Id.ToString());
            _writer.WriteString("name", role.Name);
            _writer.WriteString("color", role.Color?.ToHex());
            _writer.WriteNumber("position", role.Position);

            _writer.WriteEndObject();
        }

        _writer.WriteEndArray();
        await _writer.FlushAsync(cancellationToken);
    }

    private async ValueTask WriteEmbedAuthorAsync(
        EmbedAuthor embedAuthor,
        CancellationToken cancellationToken = default
    )
    {
        _writer.WriteStartObject();

        _writer.WriteString("name", embedAuthor.Name);
        _writer.WriteString("url", embedAuthor.Url);

        if (!string.IsNullOrWhiteSpace(embedAuthor.IconUrl))
        {
            _writer.WriteString(
                "iconUrl",
                await Context.ResolveAssetUrlAsync(
                    embedAuthor.IconProxyUrl ?? embedAuthor.IconUrl,
                    cancellationToken
                )
            );
        }

        _writer.WriteEndObject();
        await _writer.FlushAsync(cancellationToken);
    }

    private async ValueTask WriteEmbedImageAsync(
        EmbedImage embedImage,
        CancellationToken cancellationToken = default
    )
    {
        _writer.WriteStartObject();

        if (!string.IsNullOrWhiteSpace(embedImage.Url))
        {
            _writer.WriteString(
                "url",
                await Context.ResolveAssetUrlAsync(
                    embedImage.ProxyUrl ?? embedImage.Url,
                    cancellationToken
                )
            );
        }

        _writer.WriteNumber("width", embedImage.Width);
        _writer.WriteNumber("height", embedImage.Height);

        _writer.WriteEndObject();
        await _writer.FlushAsync(cancellationToken);
    }

    private async ValueTask WriteEmbedVideoAsync(
        EmbedVideo embedVideo,
        CancellationToken cancellationToken = default
    )
    {
        _writer.WriteStartObject();

        if (!string.IsNullOrWhiteSpace(embedVideo.Url))
        {
            _writer.WriteString(
                "url",
                await Context.ResolveAssetUrlAsync(
                    embedVideo.ProxyUrl ?? embedVideo.Url,
                    cancellationToken
                )
            );
        }

        _writer.WriteNumber("width", embedVideo.Width);
        _writer.WriteNumber("height", embedVideo.Height);

        _writer.WriteEndObject();
        await _writer.FlushAsync(cancellationToken);
    }

    private async ValueTask WriteEmbedFooterAsync(
        EmbedFooter embedFooter,
        CancellationToken cancellationToken = default
    )
    {
        _writer.WriteStartObject();

        _writer.WriteString("text", embedFooter.Text);

        if (!string.IsNullOrWhiteSpace(embedFooter.IconUrl))
        {
            _writer.WriteString(
                "iconUrl",
                await Context.ResolveAssetUrlAsync(
                    embedFooter.IconProxyUrl ?? embedFooter.IconUrl,
                    cancellationToken
                )
            );
        }

        _writer.WriteEndObject();
        await _writer.FlushAsync(cancellationToken);
    }

    private async ValueTask WriteEmbedFieldAsync(
        EmbedField embedField,
        CancellationToken cancellationToken = default
    )
    {
        _writer.WriteStartObject();

        _writer.WriteString("name", await FormatMarkdownAsync(embedField.Name, cancellationToken));
        _writer.WriteString(
            "value",
            await FormatMarkdownAsync(embedField.Value, cancellationToken)
        );
        _writer.WriteBoolean("isInline", embedField.IsInline);

        _writer.WriteEndObject();
        await _writer.FlushAsync(cancellationToken);
    }

    private async ValueTask WriteEmbedAsync(
        Embed embed,
        CancellationToken cancellationToken = default
    )
    {
        _writer.WriteStartObject();

        _writer.WriteString(
            "title",
            await FormatMarkdownAsync(embed.Title ?? "", cancellationToken)
        );
        _writer.WriteString("url", embed.Url);
        _writer.WriteString("timestamp", embed.Timestamp?.Pipe(Context.NormalizeDate));
        _writer.WriteString(
            "description",
            await FormatMarkdownAsync(embed.Description ?? "", cancellationToken)
        );

        if (embed.Color is not null)
            _writer.WriteString("color", embed.Color.Value.ToHex());

        if (embed.Author is not null)
        {
            _writer.WritePropertyName("author");
            await WriteEmbedAuthorAsync(embed.Author, cancellationToken);
        }

        if (embed.Thumbnail is not null)
        {
            _writer.WritePropertyName("thumbnail");
            await WriteEmbedImageAsync(embed.Thumbnail, cancellationToken);
        }

        if (embed.Image is not null)
        {
            _writer.WritePropertyName("image");
            await WriteEmbedImageAsync(embed.Image, cancellationToken);
        }

        if (embed.Video is not null)
        {
            _writer.WritePropertyName("video");
            await WriteEmbedVideoAsync(embed.Video, cancellationToken);
        }

        if (embed.Footer is not null)
        {
            _writer.WritePropertyName("footer");
            await WriteEmbedFooterAsync(embed.Footer, cancellationToken);
        }

        // Images
        _writer.WriteStartArray("images");

        foreach (var image in embed.Images)
            await WriteEmbedImageAsync(image, cancellationToken);

        _writer.WriteEndArray();

        // Fields
        _writer.WriteStartArray("fields");

        foreach (var field in embed.Fields)
            await WriteEmbedFieldAsync(field, cancellationToken);

        _writer.WriteEndArray();

        _writer.WriteEndObject();
        await _writer.FlushAsync(cancellationToken);
    }

    public override async ValueTask WritePreambleAsync(
        CancellationToken cancellationToken = default
    )
    {
        // Root object (start)
        _writer.WriteStartObject();

        // Guild
        _writer.WriteStartObject("guild");
        _writer.WriteString("id", Context.Request.Guild.Id.ToString());
        _writer.WriteString("name", Context.Request.Guild.Name);

        _writer.WriteString(
            "iconUrl",
            await Context.ResolveAssetUrlAsync(Context.Request.Guild.IconUrl, cancellationToken)
        );

        _writer.WriteEndObject();

        // Channel
        _writer.WriteStartObject("channel");
        _writer.WriteString("id", Context.Request.Channel.Id.ToString());
        _writer.WriteString("type", Context.Request.Channel.Kind.ToString());

        // Original schema did not account for threads, so 'category' actually refers to the parent channel
        _writer.WriteString("categoryId", Context.Request.Channel.Parent?.Id.ToString());
        _writer.WriteString("category", Context.Request.Channel.Parent?.Name);

        _writer.WriteString("name", Context.Request.Channel.Name);
        _writer.WriteString("topic", Context.Request.Channel.Topic);

        if (!string.IsNullOrWhiteSpace(Context.Request.Channel.IconUrl))
        {
            _writer.WriteString(
                "iconUrl",
                await Context.ResolveAssetUrlAsync(
                    Context.Request.Channel.IconUrl,
                    cancellationToken
                )
            );
        }

        _writer.WriteEndObject();

        // Date range
        _writer.WriteStartObject("dateRange");
        _writer.WriteString("after", Context.Request.After?.ToDate().Pipe(Context.NormalizeDate));
        _writer.WriteString("before", Context.Request.Before?.ToDate().Pipe(Context.NormalizeDate));
        _writer.WriteEndObject();

        // Timestamp
        _writer.WriteString("exportedAt", Context.NormalizeDate(DateTimeOffset.UtcNow));

        // Message array (start)
        _writer.WriteStartArray("messages");
        await _writer.FlushAsync(cancellationToken);
    }

    public override async ValueTask WriteMessageAsync(
        Message message,
        CancellationToken cancellationToken = default
    )
    {
        await base.WriteMessageAsync(message, cancellationToken);

        _writer.WriteStartObject();

        // Metadata
        _writer.WriteString("id", message.Id.ToString());
        _writer.WriteString("type", message.Kind.ToString());
        _writer.WriteString("timestamp", Context.NormalizeDate(message.Timestamp));
        _writer.WriteString(
            "timestampEdited",
            message.EditedTimestamp?.Pipe(Context.NormalizeDate)
        );
        _writer.WriteString(
            "callEndedTimestamp",
            message.CallEndedTimestamp?.Pipe(Context.NormalizeDate)
        );
        _writer.WriteBoolean("isPinned", message.IsPinned);

        // Content
        if (message.IsSystemNotification)
        {
            _writer.WriteString("content", message.GetFallbackContent());
        }
        else
        {
            _writer.WriteString(
                "content",
                await FormatMarkdownAsync(message.Content, cancellationToken)
            );
        }

        // Author
        _writer.WritePropertyName("author");
        await WriteUserAsync(message.Author, cancellationToken);

        // Attachments
        _writer.WriteStartArray("attachments");

        foreach (var attachment in message.Attachments)
        {
            _writer.WriteStartObject();

            _writer.WriteString("id", attachment.Id.ToString());
            _writer.WriteString(
                "url",
                await Context.ResolveAssetUrlAsync(attachment.Url, cancellationToken)
            );
            _writer.WriteString("fileName", attachment.FileName);
            _writer.WriteNumber("fileSizeBytes", attachment.FileSize.TotalBytes);

            _writer.WriteEndObject();
        }

        _writer.WriteEndArray();

        // Embeds
        _writer.WriteStartArray("embeds");

        foreach (var embed in message.Embeds)
            await WriteEmbedAsync(embed, cancellationToken);

        _writer.WriteEndArray();

        // Stickers
        _writer.WriteStartArray("stickers");

        foreach (var sticker in message.Stickers)
        {
            _writer.WriteStartObject();

            _writer.WriteString("id", sticker.Id.ToString());
            _writer.WriteString("name", sticker.Name);
            _writer.WriteString("format", sticker.Format.ToString());
            _writer.WriteString(
                "sourceUrl",
                await Context.ResolveAssetUrlAsync(sticker.SourceUrl, cancellationToken)
            );

            _writer.WriteEndObject();
        }

        _writer.WriteEndArray();

        // Reactions
        _writer.WriteStartArray("reactions");

        foreach (var reaction in message.Reactions)
        {
            _writer.WriteStartObject();

            // Emoji
            _writer.WriteStartObject("emoji");
            _writer.WriteString("id", reaction.Emoji.Id.ToString());
            _writer.WriteString("name", reaction.Emoji.Name);
            _writer.WriteString("code", reaction.Emoji.Code);
            _writer.WriteBoolean("isAnimated", reaction.Emoji.IsAnimated);
            _writer.WriteString(
                "imageUrl",
                await Context.ResolveAssetUrlAsync(reaction.Emoji.ImageUrl, cancellationToken)
            );
            _writer.WriteEndObject();

            _writer.WriteNumber("count", reaction.Count);

            _writer.WriteStartArray("users");
            await foreach (
                var user in Context.Discord.GetMessageReactionsAsync(
                    Context.Request.Channel.Id,
                    message.Id,
                    reaction.Emoji,
                    cancellationToken
                )
            )
            {
                _writer.WriteStartObject();

                // Write limited user information without color and roles,
                // so we can avoid fetching guild member information for each user.
                _writer.WriteString("id", user.Id.ToString());
                _writer.WriteString("name", user.Name);
                _writer.WriteString("discriminator", user.DiscriminatorFormatted);
                _writer.WriteString(
                    "nickname",
                    Context.TryGetMember(user.Id)?.DisplayName ?? user.DisplayName
                );
                _writer.WriteBoolean("isBot", user.IsBot);

                _writer.WriteString(
                    "avatarUrl",
                    await Context.ResolveAssetUrlAsync(
                        Context.TryGetMember(user.Id)?.AvatarUrl ?? user.AvatarUrl,
                        cancellationToken
                    )
                );

                _writer.WriteEndObject();
            }

            _writer.WriteEndArray();

            _writer.WriteEndObject();
        }

        _writer.WriteEndArray();

        // Mentions
        _writer.WriteStartArray("mentions");

        foreach (var user in message.MentionedUsers)
            await WriteUserAsync(user, cancellationToken);

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

        // Interaction
        if (message.Interaction is not null)
        {
            _writer.WriteStartObject("interaction");

            _writer.WriteString("id", message.Interaction.Id.ToString());
            _writer.WriteString("name", message.Interaction.Name);

            _writer.WritePropertyName("user");
            await WriteUserAsync(message.Interaction.User, cancellationToken);

            _writer.WriteEndObject();
        }

        _writer.WriteEndObject();
        await _writer.FlushAsync(cancellationToken);
    }

    public override async ValueTask WritePostambleAsync(
        CancellationToken cancellationToken = default
    )
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

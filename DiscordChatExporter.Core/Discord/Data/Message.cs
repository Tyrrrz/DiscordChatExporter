using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Discord.Data.Components;
using DiscordChatExporter.Core.Discord.Data.Embeds;
using DiscordChatExporter.Core.Discord.Data.Polls;
using PowerKit.Extensions;
using PowerKit.Json.Extensions;

namespace DiscordChatExporter.Core.Discord.Data;

// https://discord.com/developers/docs/resources/channel#message-object
public partial record Message(
    Snowflake Id,
    MessageKind Kind,
    MessageFlags Flags,
    User Author,
    DateTimeOffset Timestamp,
    DateTimeOffset? EditedTimestamp,
    DateTimeOffset? CallEndedTimestamp,
    bool IsPinned,
    string Content,
    IReadOnlyList<Attachment> Attachments,
    IReadOnlyList<Embed> Embeds,
    IReadOnlyList<MessageComponent> Components,
    IReadOnlyList<Sticker> Stickers,
    IReadOnlyList<Reaction> Reactions,
    IReadOnlyList<User> MentionedUsers,
    MessageReference? Reference,
    Message? ReferencedMessage,
    MessageSnapshot? ForwardedMessage,
    Interaction? Interaction,
    Poll? Poll
) : IHasId
{
    public bool IsEmpty { get; } =
        string.IsNullOrWhiteSpace(Content)
        && !Attachments.Any()
        && !Embeds.Any()
        && !Components.Any()
        && !Stickers.Any()
        && Poll is null;

    public bool IsSystemNotification { get; } =
        Kind
        is (>= MessageKind.RecipientAdd and <= MessageKind.ThreadCreated)
            or MessageKind.PollResult;

    public bool IsReply { get; } = Kind == MessageKind.Reply;

    // App interactions are rendered as replies in the Discord client, but they are not actually replies
    public bool IsReplyLike => IsReply || Interaction is not null;

    public bool IsForwarded { get; } = Reference?.Kind == MessageReferenceKind.Forward;

    // Media referenced by components (e.g. media galleries) is uploaded as message attachments,
    // but the Discord client only renders it inside the component, not as a separate attachment.
    public IReadOnlyList<Attachment> UnreferencedAttachments { get; } =
        Attachments.Where(a => !GetComponentMedia(Components).Any(m => m.Refers(a))).ToArray();

    // Text of all text display components, in display order
    public IEnumerable<string> GetComponentTexts() =>
        Components
            .SelectMany(c => c.Flatten())
            .OfType<TextDisplayComponent>()
            .Select(t => t.Content)
            .Where(t => !string.IsNullOrWhiteSpace(t));

    // Message attachment that the specified component media item points at, if any
    public Attachment? TryGetComponentMediaAttachment(UnfurledMediaItem media) =>
        Attachments.FirstOrDefault(media.Refers);

    // Media items may reference message attachments (e.g. 'attachment://file.png'),
    // in which case the attachment URL is used, same as for regular attachments.
    public string ResolveComponentMediaUrl(UnfurledMediaItem media) =>
        TryGetComponentMediaAttachment(media)?.Url ?? media.ProxyUrl ?? media.Url;

    private static IEnumerable<UnfurledMediaItem> GetComponentMedia(
        IReadOnlyList<MessageComponent> components
    ) =>
        components
            .SelectMany(c => c.Flatten())
            .SelectMany(c =>
                c switch
                {
                    ThumbnailComponent { Media: { } media } => [media],
                    FileComponent { File: { } file } => [file],
                    MediaGalleryComponent gallery => gallery.Items.Select(i => i.Media),
                    _ => [],
                }
            );

    public IEnumerable<User> GetReferencedUsers()
    {
        yield return Author;

        foreach (var user in MentionedUsers)
            yield return user;

        if (ReferencedMessage is not null)
            yield return ReferencedMessage.Author;

        if (Interaction is not null)
            yield return Interaction.User;
    }
}

public partial record Message
{
    private static IReadOnlyList<Embed> NormalizeEmbeds(IReadOnlyList<Embed> embeds)
    {
        if (embeds.Count <= 1)
            return embeds;

        // Discord API doesn't support embeds with multiple images, even though Discord client does.
        // To work around this, it seems that the API returns multiple consecutive embeds with different images,
        // which are then merged together on the client. We need to replicate the same behavior ourselves.
        // Currently, only known case where this workaround is required is Twitter embeds.
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/695

        var normalizedEmbeds = new List<Embed>();

        for (var i = 0; i < embeds.Count; i++)
        {
            var embed = embeds[i];

            if (embed.Url?.Contains("://twitter.com/", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Find embeds with the same URL that only contain a single image and nothing else
                var trailingEmbeds = embeds
                    .Skip(i + 1)
                    .TakeWhile(e =>
                        e.Url == embed.Url
                        && e.Timestamp is null
                        && e.Author is null
                        && e.Color is null
                        && string.IsNullOrWhiteSpace(e.Description)
                        && !e.Fields.Any()
                        && e.Images.Count == 1
                        && e.Footer is null
                    )
                    .ToArray();

                if (trailingEmbeds.Any())
                {
                    // Concatenate all images into one embed
                    var images = embed
                        .Images.Concat(trailingEmbeds.SelectMany(e => e.Images))
                        .ToArray();

                    normalizedEmbeds.Add(embed with { Images = images });

                    i += trailingEmbeds.Length;
                }
                else
                {
                    normalizedEmbeds.Add(embed);
                }
            }
            else
            {
                normalizedEmbeds.Add(embed);
            }
        }

        return normalizedEmbeds;
    }

    public static Message Parse(JsonElement json)
    {
        var id = json.GetProperty("id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
        var kind = json.GetProperty("type").GetInt32().Pipe(t => (MessageKind)t);

        var flags =
            json.GetPropertyOrNull("flags")?.GetInt32OrNull()?.Pipe(f => (MessageFlags)f)
            ?? MessageFlags.None;

        var author = json.GetProperty("author").Pipe(User.Parse);
        var timestamp = json.GetProperty("timestamp").GetDateTimeOffset();
        var editedTimestamp = json.GetPropertyOrNull("edited_timestamp")?.GetDateTimeOffsetOrNull();
        var callEndedTimestamp = json.GetPropertyOrNull("call")
            ?.GetPropertyOrNull("ended_timestamp")
            ?.GetDateTimeOffsetOrNull();

        var isPinned = json.GetPropertyOrNull("pinned")?.GetBooleanOrNull() ?? false;
        var content = json.GetPropertyOrNull("content")?.GetStringOrNull() ?? "";

        var attachments =
            json.GetPropertyOrNull("attachments")
                ?.EnumerateArrayOrNull()
                ?.Select(Attachment.Parse)
                .ToArray()
            ?? [];

        var embeds = NormalizeEmbeds(
            json.GetPropertyOrNull("embeds")?.EnumerateArrayOrNull()?.Select(Embed.Parse).ToArray()
                ?? []
        );

        var components = MessageComponent.ParseMany(json.GetPropertyOrNull("components"));

        var stickers =
            json.GetPropertyOrNull("sticker_items")
                ?.EnumerateArrayOrNull()
                ?.Select(Sticker.Parse)
                .ToArray()
            ?? [];

        var reactions =
            json.GetPropertyOrNull("reactions")
                ?.EnumerateArrayOrNull()
                ?.Select(Reaction.Parse)
                .ToArray()
            ?? [];

        var mentionedUsers =
            json.GetPropertyOrNull("mentions")?.EnumerateArrayOrNull()?.Select(User.Parse).ToArray()
            ?? [];

        var messageReference = json.GetPropertyOrNull("message_reference")
            ?.Pipe(MessageReference.Parse);

        var referencedMessage = json.GetPropertyOrNull("referenced_message")?.Pipe(Parse);

        // Currently Discord only supports 1 snapshot per forward
        var forwardedMessage = json.GetPropertyOrNull("message_snapshots")
            ?.EnumerateArrayOrNull()
            ?.Select(j => j.GetPropertyOrNull("message"))
            .WhereNotNull()
            .Select(MessageSnapshot.Parse)
            .FirstOrDefault();

        var interaction = json.GetPropertyOrNull("interaction")?.Pipe(Interaction.Parse);

        var poll = json.GetPropertyOrNull("poll")?.Pipe(Poll.Parse);

        return new Message(
            id,
            kind,
            flags,
            author,
            timestamp,
            editedTimestamp,
            callEndedTimestamp,
            isPinned,
            content,
            attachments,
            embeds,
            components,
            stickers,
            reactions,
            mentionedUsers,
            messageReference,
            referencedMessage,
            forwardedMessage,
            interaction,
            poll
        );
    }
}

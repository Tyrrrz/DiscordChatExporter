using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Discord.Data.Embeds;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

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
    IReadOnlyList<Sticker> Stickers,
    IReadOnlyList<Reaction> Reactions,
    IReadOnlyList<User> MentionedUsers,
    MessageReference? Reference,
    Message? ReferencedMessage,
    Interaction? Interaction
) : IHasId
{
    public bool IsReplyLike => Kind == MessageKind.Reply || Interaction is not null;

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
                    .TakeWhile(
                        e =>
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
                    var images = embed.Images
                        .Concat(trailingEmbeds.SelectMany(e => e.Images))
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
        var kind = (MessageKind)json.GetProperty("type").GetInt32();
        var flags =
            (MessageFlags?)json.GetPropertyOrNull("flags")?.GetInt32OrNull() ?? MessageFlags.None;
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
                .ToArray() ?? Array.Empty<Attachment>();

        var embeds = NormalizeEmbeds(
            json.GetPropertyOrNull("embeds")?.EnumerateArrayOrNull()?.Select(Embed.Parse).ToArray()
                ?? Array.Empty<Embed>()
        );

        var stickers =
            json.GetPropertyOrNull("sticker_items")
                ?.EnumerateArrayOrNull()
                ?.Select(Sticker.Parse)
                .ToArray() ?? Array.Empty<Sticker>();

        var reactions =
            json.GetPropertyOrNull("reactions")
                ?.EnumerateArrayOrNull()
                ?.Select(Reaction.Parse)
                .ToArray() ?? Array.Empty<Reaction>();

        var mentionedUsers =
            json.GetPropertyOrNull("mentions")?.EnumerateArrayOrNull()?.Select(User.Parse).ToArray()
            ?? Array.Empty<User>();

        var messageReference = json.GetPropertyOrNull("message_reference")
            ?.Pipe(MessageReference.Parse);
        var referencedMessage = json.GetPropertyOrNull("referenced_message")?.Pipe(Parse);
        var interaction = json.GetPropertyOrNull("interaction")?.Pipe(Interaction.Parse);

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
            stickers,
            reactions,
            mentionedUsers,
            messageReference,
            referencedMessage,
            interaction
        );
    }
}

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
public record Message(
    Snowflake Id,
    MessageKind Kind,
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
    Message? ReferencedMessage) : IHasId
{
    public static Message Parse(JsonElement json)
    {
        var id = json.GetProperty("id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
        var author = json.GetProperty("author").Pipe(User.Parse);
        var timestamp = json.GetProperty("timestamp").GetDateTimeOffset();
        var editedTimestamp = json.GetPropertyOrNull("edited_timestamp")?.GetDateTimeOffset();
        var callEndedTimestamp = json.GetPropertyOrNull("call")?.GetPropertyOrNull("ended_timestamp")
            ?.GetDateTimeOffset();
        var kind = (MessageKind)json.GetProperty("type").GetInt32();
        var isPinned = json.GetPropertyOrNull("pinned")?.GetBooleanOrNull() ?? false;
        var messageReference = json.GetPropertyOrNull("message_reference")?.Pipe(MessageReference.Parse);
        var referencedMessage = json.GetPropertyOrNull("referenced_message")?.Pipe(Parse);

        var content = kind switch
        {
            MessageKind.RecipientAdd => "Added a recipient.",
            MessageKind.RecipientRemove => "Removed a recipient.",
            MessageKind.Call =>
                $"Started a call that lasted {callEndedTimestamp?.Pipe(t => t - timestamp).Pipe(t => (int)t.TotalMinutes) ?? 0} minutes.",
            MessageKind.ChannelNameChange => "Changed the channel name.",
            MessageKind.ChannelIconChange => "Changed the channel icon.",
            MessageKind.ChannelPinnedMessage => "Pinned a message.",
            MessageKind.GuildMemberJoin => "Joined the server.",
            _ => json.GetPropertyOrNull("content")?.GetStringOrNull() ?? ""
        };

        var attachments =
            json.GetPropertyOrNull("attachments")?.EnumerateArrayOrNull()?.Select(Attachment.Parse).ToArray() ??
            Array.Empty<Attachment>();

        var embeds =
            json.GetPropertyOrNull("embeds")?.EnumerateArrayOrNull()?.Select(Embed.Parse).ToArray() ??
            Array.Empty<Embed>();

        var stickers =
            json.GetPropertyOrNull("sticker_items")?.EnumerateArrayOrNull()?.Select(Sticker.Parse).ToArray() ??
            Array.Empty<Sticker>();

        var reactions =
            json.GetPropertyOrNull("reactions")?.EnumerateArrayOrNull()?.Select(Reaction.Parse).ToArray() ??
            Array.Empty<Reaction>();

        var mentionedUsers =
            json.GetPropertyOrNull("mentions")?.EnumerateArrayOrNull()?.Select(User.Parse).ToArray() ??
            Array.Empty<User>();

        return new Message(
            id,
            kind,
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
            referencedMessage
        );
    }
}
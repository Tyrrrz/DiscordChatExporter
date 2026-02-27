using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Embeds;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data;

// https://discord.com/developers/docs/resources/channel#message-snapshot-object
// Message snapshots contain a subset of message fields for forwarded messages
public record MessageSnapshot(
    string Content,
    IReadOnlyList<Attachment> Attachments,
    IReadOnlyList<Embed> Embeds,
    IReadOnlyList<Sticker> Stickers,
    DateTimeOffset Timestamp,
    DateTimeOffset? EditedTimestamp
)
{
    public static MessageSnapshot Parse(JsonElement json)
    {
        // The message snapshot has a "message" property containing the actual message data
        var messageJson = json.GetPropertyOrNull("message") ?? json;

        var content = messageJson.GetPropertyOrNull("content")?.GetStringOrNull() ?? "";

        var attachments =
            messageJson
                .GetPropertyOrNull("attachments")
                ?.EnumerateArrayOrNull()
                ?.Select(Attachment.Parse)
                .ToArray()
            ?? [];

        var embeds =
            messageJson
                .GetPropertyOrNull("embeds")
                ?.EnumerateArrayOrNull()
                ?.Select(Embed.Parse)
                .ToArray()
            ?? [];

        var stickers =
            messageJson
                .GetPropertyOrNull("sticker_items")
                ?.EnumerateArrayOrNull()
                ?.Select(Sticker.Parse)
                .ToArray()
            ?? [];

        var timestamp =
            messageJson.GetPropertyOrNull("timestamp")?.GetDateTimeOffsetOrNull()
            ?? DateTimeOffset.MinValue;

        var editedTimestamp = messageJson
            .GetPropertyOrNull("edited_timestamp")
            ?.GetDateTimeOffsetOrNull();

        return new MessageSnapshot(
            content,
            attachments,
            embeds,
            stickers,
            timestamp,
            editedTimestamp
        );
    }
}

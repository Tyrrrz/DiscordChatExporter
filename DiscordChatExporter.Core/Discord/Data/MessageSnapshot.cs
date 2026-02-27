using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Embeds;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data;

// https://docs.discord.com/developers/resources/message#message-snapshot-object
public record MessageSnapshot(
    DateTimeOffset Timestamp,
    DateTimeOffset? EditedTimestamp,
    string Content,
    IReadOnlyList<Attachment> Attachments,
    IReadOnlyList<Embed> Embeds,
    IReadOnlyList<Sticker> Stickers
)
{
    public static MessageSnapshot Parse(JsonElement json)
    {
        var timestamp =
            json.GetPropertyOrNull("timestamp")?.GetDateTimeOffsetOrNull()
            ?? DateTimeOffset.MinValue;

        var editedTimestamp = json.GetPropertyOrNull("edited_timestamp")?.GetDateTimeOffsetOrNull();

        var content = json.GetPropertyOrNull("content")?.GetStringOrNull() ?? "";

        var attachments =
            json.GetPropertyOrNull("attachments")
                ?.EnumerateArrayOrNull()
                ?.Select(Attachment.Parse)
                .ToArray()
            ?? [];

        var embeds =
            json.GetPropertyOrNull("embeds")?.EnumerateArrayOrNull()?.Select(Embed.Parse).ToArray()
            ?? [];

        var stickers =
            json.GetPropertyOrNull("sticker_items")
                ?.EnumerateArrayOrNull()
                ?.Select(Sticker.Parse)
                .ToArray()
            ?? [];

        return new MessageSnapshot(
            timestamp,
            editedTimestamp,
            content,
            attachments,
            embeds,
            stickers
        );
    }
}

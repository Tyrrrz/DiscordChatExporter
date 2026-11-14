using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using PowerKit.Extensions;
using PowerKit.Json.Extensions;

namespace DiscordChatExporter.Core.Discord.Data.Components;

// https://discord.com/developers/docs/components/reference#file
public record FileComponent(UnfurledMediaItem? File, string? Name, FileSize? Size, bool IsSpoiler)
    : MessageComponent(MessageComponentKind.File)
{
    public static FileComponent Parse(JsonElement json) =>
        new(
            json.GetPropertyOrNull("file")?.Pipe(UnfurledMediaItem.TryParse),
            json.GetPropertyOrNull("name")?.GetNonWhiteSpaceStringOrNull(),
            json.GetPropertyOrNull("size")?.GetInt64OrNull()?.Pipe(FileSize.FromBytes),
            json.GetPropertyOrNull("spoiler")?.GetBooleanOrNull() ?? false
        );
}

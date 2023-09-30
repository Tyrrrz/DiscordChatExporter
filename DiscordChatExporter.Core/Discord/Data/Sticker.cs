using System;
using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data;

// https://discord.com/developers/docs/resources/sticker#sticker-resource
public record Sticker(Snowflake Id, string Name, StickerFormat Format, string SourceUrl)
{
    public static Sticker Parse(JsonElement json)
    {
        var id = json.GetProperty("id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
        var name = json.GetProperty("name").GetNonNullString();
        var format = json.GetProperty("format_type").GetInt32().Pipe(t => (StickerFormat)t);

        var sourceUrl = ImageCdn.GetStickerUrl(
            id,
            format switch
            {
                StickerFormat.Png => "png",
                StickerFormat.Apng => "png",
                StickerFormat.Lottie => "json",
                _ => throw new InvalidOperationException($"Unknown sticker format '{format}'.")
            }
        );

        return new Sticker(id, name, format, sourceUrl);
    }
}

using System;
using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data;

// https://discord.com/developers/docs/resources/sticker#sticker-resource
public partial record Sticker(Snowflake Id, string Name, StickerFormat Format, string SourceUrl)
{
    public bool IsImage => Format != StickerFormat.Lottie;
}

public partial record Sticker
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
                StickerFormat.Gif => "gif",
                _ => throw new InvalidOperationException($"Unknown sticker format '{format}'.")
            }
        );

        return new Sticker(id, name, format, sourceUrl);
    }
}

using System.Text.Json;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data;

// https://discord.com/developers/docs/resources/sticker#sticker-resource
public record Sticker(Snowflake Id, string Name, StickerFormat Format, string SourceUrl)
{
    private static string GetSourceUrl(Snowflake id, StickerFormat format)
    {
        var extension = format == StickerFormat.Lottie ? "json" : "png";
        return $"https://discord.com/stickers/{id}.{extension}";
    }

    public static Sticker Parse(JsonElement json)
    {
        var id = json.GetProperty("id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
        var name = json.GetProperty("name").GetNonNullString();
        var format = (StickerFormat)json.GetProperty("format_type").GetInt32();
        var sourceUrl = GetSourceUrl(id, format);

        return new Sticker(id, name, format, sourceUrl);
    }
}
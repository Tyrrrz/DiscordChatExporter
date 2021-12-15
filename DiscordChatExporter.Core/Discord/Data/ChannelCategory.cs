using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data;

public record ChannelCategory(Snowflake Id, string Name, int? Position) : IHasId
{
    public static ChannelCategory Unknown { get; } = new(Snowflake.Zero, "<unknown category>", 0);

    public static ChannelCategory Parse(JsonElement json, int? positionHint = null)
    {
        var id = json.GetProperty("id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
        var position = positionHint ?? json.GetPropertyOrNull("position")?.GetInt32OrNull();

        var name =
            json.GetPropertyOrNull("name")?.GetNonWhiteSpaceStringOrNull() ??
            id.ToString();

        return new ChannelCategory(id, name, position);
    }
}
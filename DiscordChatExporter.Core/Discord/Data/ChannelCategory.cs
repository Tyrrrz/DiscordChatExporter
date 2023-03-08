using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data;

public record ChannelCategory(Snowflake Id, string Name, int? Position) : IHasId
{
    public static ChannelCategory CreateDefault(ChannelKind channelKind) => new(
        Snowflake.Zero,
        channelKind switch
        {
            ChannelKind.GuildTextChat => "Text",
            ChannelKind.DirectTextChat => "Private",
            ChannelKind.DirectGroupTextChat => "Group",
            ChannelKind.GuildNews => "News",
            _ => "Default"
        },
        null
    );

    public static ChannelCategory Parse(JsonElement json, int? positionHint = null)
    {
        var id = json.GetProperty("id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);

        var name =
            json.GetPropertyOrNull("name")?.GetNonWhiteSpaceStringOrNull() ??
            id.ToString();

        var position =
            positionHint ??
            json.GetPropertyOrNull("position")?.GetInt32OrNull();

        return new ChannelCategory(id, name, position);
    }
}
using System.Linq;
using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data;

// https://discord.com/developers/docs/resources/channel#channel-object
public partial record Channel(
    Snowflake Id,
    ChannelKind Kind,
    Snowflake GuildId,
    ChannelCategory Category,
    string Name,
    int? Position,
    string? Topic) : IHasId
{
    public bool IsTextChannel => Kind is
        ChannelKind.GuildTextChat or
        ChannelKind.DirectTextChat or
        ChannelKind.DirectGroupTextChat or
        ChannelKind.GuildNews or
        ChannelKind.GuildStore;

    public bool IsVoiceChannel => !IsTextChannel;
}

public partial record Channel
{
    private static ChannelCategory GetFallbackCategory(ChannelKind channelKind) => new(
        Snowflake.Zero,
        channelKind switch
        {
            ChannelKind.GuildTextChat => "Text",
            ChannelKind.DirectTextChat => "Private",
            ChannelKind.DirectGroupTextChat => "Group",
            ChannelKind.GuildNews => "News",
            ChannelKind.GuildStore => "Store",
            _ => "Default"
        },
        null
    );

    public static Channel Parse(JsonElement json, ChannelCategory? category = null, int? positionHint = null)
    {
        var id = json.GetProperty("id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
        var guildId = json.GetPropertyOrNull("guild_id")?.GetNonWhiteSpaceStringOrNull()?.Pipe(Snowflake.Parse);
        var topic = json.GetPropertyOrNull("topic")?.GetStringOrNull();
        var kind = (ChannelKind)json.GetProperty("type").GetInt32();

        var name =
            // Guild channel
            json.GetPropertyOrNull("name")?.GetNonWhiteSpaceStringOrNull() ??

            // DM channel
            json.GetPropertyOrNull("recipients")?
                .EnumerateArrayOrNull()?
                .Select(User.Parse)
                .Select(u => u.Name)
                .Pipe(s => string.Join(", ", s)) ??

            // Fallback
            id.ToString();

        var position = positionHint ?? json.GetPropertyOrNull("position")?.GetInt32OrNull();

        return new Channel(
            id,
            kind,
            guildId ?? Guild.DirectMessages.Id,
            category ?? GetFallbackCategory(kind),
            name,
            position,
            topic
        );
    }
}
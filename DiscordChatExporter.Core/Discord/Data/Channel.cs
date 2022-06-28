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
    string? Topic,
    Snowflake? LastMessageId) : IHasId
{
    public bool SupportsVoice => Kind is ChannelKind.GuildVoiceChat or ChannelKind.GuildStageVoice;
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
            _ => "Default"
        },
        null
    );

    public static Channel Parse(JsonElement json, ChannelCategory? category = null, int? positionHint = null)
    {
        var id = json.GetProperty("id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
        var kind = (ChannelKind)json.GetProperty("type").GetInt32();
        var guildId = json.GetPropertyOrNull("guild_id")?.GetNonWhiteSpaceStringOrNull()?.Pipe(Snowflake.Parse);

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

        var position =
            positionHint ??
            json.GetPropertyOrNull("position")?.GetInt32OrNull();

        var topic = json.GetPropertyOrNull("topic")?.GetStringOrNull();

        var lastMessageId = json
            .GetPropertyOrNull("last_message_id")?
            .GetNonWhiteSpaceStringOrNull()?
            .Pipe(Snowflake.Parse);

        return new Channel(
            id,
            kind,
            guildId ?? Guild.DirectMessages.Id,
            category ?? GetFallbackCategory(kind),
            name,
            position,
            topic,
            lastMessageId
        );
    }
}
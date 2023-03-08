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
    string? IconUrl,
    string? Topic,
    Snowflake? LastMessageId) : IHasId
{
    public bool IsVoice => Kind is ChannelKind.GuildVoiceChat or ChannelKind.GuildStageVoice;
}

public partial record Channel
{
    public static Channel Parse(JsonElement json, ChannelCategory? categoryHint = null, int? positionHint = null)
    {
        var id = json.GetProperty("id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
        var kind = (ChannelKind)json.GetProperty("type").GetInt32();

        var guildId =
            json.GetPropertyOrNull("guild_id")?.GetNonWhiteSpaceStringOrNull()?.Pipe(Snowflake.Parse) ??
            Guild.DirectMessages.Id;

        var category = categoryHint ?? ChannelCategory.CreateDefault(kind);

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

        // Icons can only be set for group DM channels
        var iconUrl = json
            .GetPropertyOrNull("icon")?
            .GetNonWhiteSpaceStringOrNull()?
            .Pipe(h => ImageCdn.GetChannelIconUrl(id, h));

        var topic = json.GetPropertyOrNull("topic")?.GetStringOrNull();

        var lastMessageId = json
            .GetPropertyOrNull("last_message_id")?
            .GetNonWhiteSpaceStringOrNull()?
            .Pipe(Snowflake.Parse);

        return new Channel(
            id,
            kind,
            guildId,
            category,
            name,
            position,
            iconUrl,
            topic,
            lastMessageId
        );
    }
}
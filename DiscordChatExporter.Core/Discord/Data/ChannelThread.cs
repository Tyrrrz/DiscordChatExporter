using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data;

// https://discord.com/developers/docs/resources/channel#channel-object-example-thread-channel
public record ChannelThread(
    Snowflake Id,
    ChannelKind Kind,
    Snowflake GuildId,
    string Name,
    Snowflake? LastMessageId) : IHasId
{
    public static ChannelThread Parse(JsonElement json)
    {
        var id = json.GetProperty("id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
        var kind = (ChannelKind)json.GetProperty("type").GetInt32();
        var guildId = json.GetProperty("guild_id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
        var name = json.GetProperty("name").GetNonWhiteSpaceString();

        var lastMessageId = json
            .GetPropertyOrNull("last_message_id")?
            .GetNonWhiteSpaceStringOrNull()?
            .Pipe(Snowflake.Parse);

        return new ChannelThread(
            id,
            kind,
            guildId,
            name,
            lastMessageId
        );
    }
}
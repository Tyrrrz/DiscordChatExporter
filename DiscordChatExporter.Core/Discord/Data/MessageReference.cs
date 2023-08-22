using System.Text.Json;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data;

// https://discord.com/developers/docs/resources/channel#message-object-message-reference-structure
public record MessageReference(Snowflake? MessageId, Snowflake? ChannelId, Snowflake? GuildId)
{
    public static MessageReference Parse(JsonElement json)
    {
        var messageId = json.GetPropertyOrNull("message_id")
            ?.GetNonWhiteSpaceStringOrNull()
            ?.Pipe(Snowflake.Parse);

        var channelId = json.GetPropertyOrNull("channel_id")
            ?.GetNonWhiteSpaceStringOrNull()
            ?.Pipe(Snowflake.Parse);

        var guildId = json.GetPropertyOrNull("guild_id")
            ?.GetNonWhiteSpaceStringOrNull()
            ?.Pipe(Snowflake.Parse);

        return new MessageReference(messageId, channelId, guildId);
    }
}

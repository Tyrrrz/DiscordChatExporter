using System.Linq;
using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data;

// https://discord.com/developers/docs/resources/channel#channel-object-example-thread-channel
public partial record ThreadChannel(
    Snowflake Id,
    ChannelKind Kind,
    Snowflake GuildId,
    string Name,
    Snowflake? LastMessageId) : IHasId
{

}

public partial record ThreadChannel
{
    public static ThreadChannel Parse(JsonElement json)
    {
        var id = json.GetProperty("id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
        var kind = (ChannelKind)json.GetProperty("type").GetInt32();

        var guildId =
            json.GetPropertyOrNull("guild_id")?.GetNonWhiteSpaceStringOrNull()?.Pipe(Snowflake.Parse) ?? default;

        var name =
            // Guild channel
            json.GetPropertyOrNull("name")?.GetNonWhiteSpaceStringOrNull() ??

            // Fallback
            id.ToString();

        var lastMessageId = json
            .GetPropertyOrNull("last_message_id")?
            .GetNonWhiteSpaceStringOrNull()?
            .Pipe(Snowflake.Parse);

        return new ThreadChannel(
            id,
            kind,
            guildId,
            name,
            lastMessageId
        );
    }
}
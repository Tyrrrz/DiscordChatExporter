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
    Snowflake ParentId,
    ChannelCategory Category,
    string Name,
    bool IsActive,
    Snowflake? LastMessageId) : IChannel
{
    public static ChannelThread Parse(JsonElement json, ChannelCategory? categoryHint = null)
    {
        var id = json.GetProperty("id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
        var kind = (ChannelKind)json.GetProperty("type").GetInt32();
        var guildId = json.GetProperty("guild_id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
        var parentId = json.GetProperty("parent_id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
        var category = categoryHint ?? ChannelCategory.CreateDefault(kind);
        var name = json.GetProperty("name").GetNonWhiteSpaceString();

        var isActive = !json
            .GetPropertyOrNull("thread_metadata")?
            .GetPropertyOrNull("archived")?
            .GetBooleanOrNull() ?? true;

        var lastMessageId = json
            .GetPropertyOrNull("last_message_id")?
            .GetNonWhiteSpaceStringOrNull()?
            .Pipe(Snowflake.Parse);

        return new ChannelThread(
            id,
            kind,
            guildId,
            parentId,
            category,
            name,
            isActive,
            lastMessageId
        );
    }
}
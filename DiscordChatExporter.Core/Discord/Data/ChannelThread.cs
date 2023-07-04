﻿using System.Text.Json;
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
    string? ParentName,
    string Name,
    bool IsActive,
    Snowflake? LastMessageId) : IChannel
{
    public int? ParentPosition => null;
    public int? Position => null;
    public string? IconUrl => null;
    public string? Topic => null;

    public static ChannelThread Parse(JsonElement json, string parentName)
    {
        var id = json.GetProperty("id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
        var kind = (ChannelKind)json.GetProperty("type").GetInt32();
        var guildId = json.GetProperty("guild_id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
        var parentId = json.GetProperty("parent_id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
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
            parentName,
            name,
            isActive,
            lastMessageId
        );
    }
}
using System.Collections.Generic;
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
    Channel? Parent,
    string Name,
    int? Position,
    string? IconUrl,
    string? Topic,
    bool IsArchived,
    Snowflake? LastMessageId
) : IHasId
{
    public bool IsDirect => Kind is ChannelKind.DirectTextChat or ChannelKind.DirectGroupTextChat;

    public bool IsGuild => !IsDirect;

    public bool IsCategory => Kind == ChannelKind.GuildCategory;

    public bool IsVoice => Kind is ChannelKind.GuildVoiceChat or ChannelKind.GuildStageVoice;

    public bool IsThread =>
        Kind
            is ChannelKind.GuildNewsThread
                or ChannelKind.GuildPublicThread
                or ChannelKind.GuildPrivateThread;

    public bool IsEmpty => LastMessageId is null;

    public IEnumerable<Channel> GetParents()
    {
        var current = Parent;
        while (current is not null)
        {
            yield return current;
            current = current.Parent;
        }
    }

    public Channel? TryGetRootParent() => GetParents().LastOrDefault();

    public string GetHierarchicalName() =>
        string.Join(" / ", GetParents().Reverse().Select(c => c.Name).Append(Name));

    public bool MayHaveMessagesAfter(Snowflake messageId) => !IsEmpty && messageId < LastMessageId;

    public bool MayHaveMessagesBefore(Snowflake messageId) => !IsEmpty && messageId > Id;
}

public partial record Channel
{
    public static Channel Parse(JsonElement json, Channel? parent = null, int? positionHint = null)
    {
        var id = json.GetProperty("id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
        var kind = json.GetProperty("type").GetInt32().Pipe(t => (ChannelKind)t);

        var guildId =
            json.GetPropertyOrNull("guild_id")
                ?.GetNonWhiteSpaceStringOrNull()
                ?.Pipe(Snowflake.Parse) ?? Guild.DirectMessages.Id;

        var name =
            // Guild channel
            json.GetPropertyOrNull("name")?.GetNonWhiteSpaceStringOrNull()
            // DM channel
            ?? json.GetPropertyOrNull("recipients")
                ?.EnumerateArrayOrNull()
                ?.Select(User.Parse)
                .Select(u => u.DisplayName)
                .Pipe(s => string.Join(", ", s))
            // Fallback
            ?? id.ToString();

        var position = positionHint ?? json.GetPropertyOrNull("position")?.GetInt32OrNull();

        // Icons can only be set for group DM channels
        var iconUrl = json.GetPropertyOrNull("icon")
            ?.GetNonWhiteSpaceStringOrNull()
            ?.Pipe(h => ImageCdn.GetChannelIconUrl(id, h));

        var topic = json.GetPropertyOrNull("topic")?.GetStringOrNull();

        var isArchived =
            json.GetPropertyOrNull("thread_metadata")
                ?.GetPropertyOrNull("archived")
                ?.GetBooleanOrNull() ?? false;

        var lastMessageId = json.GetPropertyOrNull("last_message_id")
            ?.GetNonWhiteSpaceStringOrNull()
            ?.Pipe(Snowflake.Parse);

        return new Channel(
            id,
            kind,
            guildId,
            parent,
            name,
            position,
            iconUrl,
            topic,
            isArchived,
            lastMessageId
        );
    }
}

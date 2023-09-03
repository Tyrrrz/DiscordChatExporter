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
    // Used for visual backwards-compatibility with old exports, where
    // channels without a parent (i.e. mostly DM channels) or channels
    // with an inaccessible parent (i.e. inside private categories) had
    // a fallback category created for them.
    public string ParentNameWithFallback =>
        Parent?.Name
        ?? Kind switch
        {
            ChannelKind.GuildCategory => "Category",
            ChannelKind.GuildTextChat => "Text",
            ChannelKind.DirectTextChat => "Private",
            ChannelKind.DirectGroupTextChat => "Group",
            ChannelKind.GuildPrivateThread => "Private Thread",
            ChannelKind.GuildPublicThread => "Public Thread",
            ChannelKind.GuildNews => "News",
            ChannelKind.GuildNewsThread => "News Thread",
            _ => "Default"
        };

    public bool IsEmpty => LastMessageId is null;

    // Only needed for WPF data binding. Don't use anywhere else.
    public bool IsVoice => Kind.IsVoice();

    // Only needed for WPF data binding. Don't use anywhere else.
    public bool IsThread => Kind.IsThread();

    public bool MayHaveMessagesAfter(Snowflake messageId) => !IsEmpty && messageId < LastMessageId;

    public bool MayHaveMessagesBefore(Snowflake messageId) => !IsEmpty && messageId > Id;
}

public partial record Channel
{
    public static Channel Parse(JsonElement json, Channel? parent = null, int? positionHint = null)
    {
        var id = json.GetProperty("id").GetNonWhiteSpaceString().Pipe(Snowflake.Parse);
        var kind = (ChannelKind)json.GetProperty("type").GetInt32();

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

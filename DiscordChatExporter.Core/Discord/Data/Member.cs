using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data;

// https://discord.com/developers/docs/resources/guild#guild-member-object
public partial record Member(
    User User,
    string? DisplayName,
    string? AvatarUrl,
    IReadOnlyList<Snowflake> RoleIds
) : IHasId
{
    public Snowflake Id => User.Id;
}

public partial record Member
{
    public static Member CreateFallback(User user) => new(user, null, null, []);

    public static Member Parse(JsonElement json, Snowflake? guildId = null)
    {
        var user = json.GetProperty("user").Pipe(User.Parse);
        var displayName = json.GetPropertyOrNull("nick")?.GetNonWhiteSpaceStringOrNull();

        var roleIds =
            json.GetPropertyOrNull("roles")
                ?.EnumerateArray()
                .Select(j => j.GetNonWhiteSpaceString())
                .Select(Snowflake.Parse)
                .ToArray() ?? [];

        var avatarUrl = guildId is not null
            ? json.GetPropertyOrNull("avatar")
                ?.GetNonWhiteSpaceStringOrNull()
                ?.Pipe(h => ImageCdn.GetMemberAvatarUrl(guildId.Value, user.Id, h))
            : null;

        return new Member(user, displayName, avatarUrl, roleIds);
    }
}

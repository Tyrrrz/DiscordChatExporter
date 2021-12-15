using System;
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
    string Nick,
    IReadOnlyList<Snowflake> RoleIds) : IHasId
{
    public Snowflake Id => User.Id;
}

public partial record Member
{
    public static Member CreateForUser(User user) => new(
        user,
        user.Name,
        Array.Empty<Snowflake>()
    );

    public static Member Parse(JsonElement json)
    {
        var user = json.GetProperty("user").Pipe(User.Parse);
        var nick = json.GetPropertyOrNull("nick")?.GetNonWhiteSpaceStringOrNull();

        var roleIds = json
            .GetPropertyOrNull("roles")?
            .EnumerateArray()
            .Select(j => j.GetNonWhiteSpaceString())
            .Select(Snowflake.Parse)
            .ToArray() ?? Array.Empty<Snowflake>();

        return new Member(
            user,
            nick ?? user.Name,
            roleIds
        );
    }
}
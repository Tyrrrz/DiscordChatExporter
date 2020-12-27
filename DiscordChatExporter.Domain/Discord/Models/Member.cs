using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DiscordChatExporter.Domain.Discord.Models.Common;
using DiscordChatExporter.Domain.Utilities;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Domain.Discord.Models
{
    // https://discord.com/developers/docs/resources/guild#guild-member-object
    public partial class Member : IHasId
    {
        public Snowflake Id => User.Id;

        public User User { get; }

        public string Nick { get; }

        public IReadOnlyList<Snowflake> RoleIds { get; }

        public Member(User user, string? nick, IReadOnlyList<Snowflake> roleIds)
        {
            User = user;
            Nick = nick ?? user.Name;
            RoleIds = roleIds;
        }

        public override string ToString() => Nick;
    }

    public partial class Member
    {
        public static Member CreateForUser(User user) => new(user, null, Array.Empty<Snowflake>());

        public static Member Parse(JsonElement json)
        {
            var user = json.GetProperty("user").Pipe(User.Parse);
            var nick = json.GetPropertyOrNull("nick")?.GetString();

            var roleIds =
                json.GetPropertyOrNull("roles")?.EnumerateArray().Select(j => j.GetString().Pipe(Snowflake.Parse)).ToArray() ??
                Array.Empty<Snowflake>();

            return new Member(
                user,
                nick,
                roleIds
            );
        }
    }
}
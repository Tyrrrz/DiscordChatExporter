using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data
{
    // https://discord.com/developers/docs/resources/guild#guild-member-object
    public partial class Member : IHasId
    {
        public Snowflake Id => User.Id;

        public User User { get; }

        public string Nick { get; }

        public IReadOnlyList<Snowflake> RoleIds { get; }

        public Member(User user, string nick, IReadOnlyList<Snowflake> roleIds)
        {
            User = user;
            Nick = nick;
            RoleIds = roleIds;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString() => Nick;
    }

    public partial class Member
    {
        public static Member CreateForUser(User user) => new(
            user,
            user.Name,
            Array.Empty<Snowflake>()
        );

        public static Member Parse(JsonElement json)
        {
            var user = json.GetProperty("user").Pipe(User.Parse);
            var nick = json.GetPropertyOrNull("nick")?.GetString();

            var roleIds =
                json.GetPropertyOrNull("roles")?.EnumerateArray().Select(j => j.GetString()).Select(Snowflake.Parse).ToArray() ??
                Array.Empty<Snowflake>();

            return new Member(
                user,
                nick ?? user.Name,
                roleIds
            );
        }
    }
}
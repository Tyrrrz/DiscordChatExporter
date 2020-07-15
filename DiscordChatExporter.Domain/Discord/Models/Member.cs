using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DiscordChatExporter.Domain.Discord.Models.Common;
using DiscordChatExporter.Domain.Internal;
using DiscordChatExporter.Domain.Internal.Extensions;

namespace DiscordChatExporter.Domain.Discord.Models
{
    // https://discordapp.com/developers/docs/resources/guild#guild-member-object
    public partial class Member : IHasId
    {
        public string Id => User.Id;

        public User User { get; }

        public string Nick { get; }

        public IReadOnlyList<string> RoleIds { get; }

        public Member(User user, string? nick, IReadOnlyList<string> roleIds)
        {
            User = user;
            Nick = nick ?? user.Name;
            RoleIds = roleIds;
        }

        public override string ToString() => Nick;
    }

    public partial class Member
    {
        public static Member CreateForUser(User user) =>
            new Member(user, null, Array.Empty<string>());

        public static Member Parse(JsonElement json)
        {
            var user = json.GetProperty("user").Pipe(User.Parse);
            var nick = json.GetPropertyOrNull("nick")?.GetString();

            var roleIds =
                json.GetPropertyOrNull("roles")?.EnumerateArray().Select(j => j.GetString()).ToArray() ??
                Array.Empty<string>();

            return new Member(
                user,
                nick,
                roleIds
            );
        }
    }
}
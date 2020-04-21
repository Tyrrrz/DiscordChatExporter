using System.Collections.Generic;

namespace DiscordChatExporter.Domain.Discord.Models
{
    // https://discordapp.com/developers/docs/resources/guild#guild-member-object

    public class Member
    {
        public string UserId { get; }

        public string? Nick { get; }

        public IReadOnlyList<string> RoleIds { get; }

        public Member(string userId, string? nick, IReadOnlyList<string> roleIds)
        {
            UserId = userId;
            Nick = nick;
            RoleIds = roleIds;
        }
    }
}
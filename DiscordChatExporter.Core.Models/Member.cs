using System.Collections.Generic;
using System.Linq;

namespace DiscordChatExporter.Core.Models
{
    public class Member
    {
        public string UserId { get; }
        public string? Nick { get; }

        public IReadOnlyList<string> Roles { get; }

        public Member(string userId, string? nick, IReadOnlyList<string> roles)
        {
            UserId = userId;
            Nick = nick;
            Roles = roles;
        }
    }
}
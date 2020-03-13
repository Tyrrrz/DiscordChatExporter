using System.Collections.Generic;
using System.Linq;

namespace DiscordChatExporter.Core.Models
{
    public class Member
    {
        public string UserId;
        public string? Nick;

        public string[] Roles { get; }

        public Member(string userId, string? nick, string[] roles)
        {
            UserId = userId;
            Nick = nick;
            Roles = roles;
        }
    }
}
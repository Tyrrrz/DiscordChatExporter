using System.Collections.Generic;

namespace DiscordChatExporter.Core.Models
{
    public class MentionContainer
    {
        public IReadOnlyList<User> Users { get; }

        public IReadOnlyList<Channel> Channels { get; }

        public IReadOnlyList<Role> Roles { get; }

        public MentionContainer(IReadOnlyList<User> users, IReadOnlyList<Channel> channels, IReadOnlyList<Role> roles)
        {
            Users = users;
            Channels = channels;
            Roles = roles;
        }
    }
}
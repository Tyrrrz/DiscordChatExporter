using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordChatExporter.Core.Models
{
    interface IMentionable
    {
        IReadOnlyList<User> MentionedUsers { get; }

        IReadOnlyList<Role> MentionedRoles { get; }

        IReadOnlyList<Channel> MentionedChannels { get; }
    }
}

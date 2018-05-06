using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordChatExporter.Core.Models
{
    interface IMentionable
    {
        List<User> MentionedUsers { get; }

        List<Role> MentionedRoles { get; }

        List<Channel> MentionedChannels { get; }
    }
}

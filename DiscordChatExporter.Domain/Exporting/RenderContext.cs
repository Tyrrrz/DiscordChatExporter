using System;
using System.Collections.Generic;
using DiscordChatExporter.Domain.Discord.Models;

namespace DiscordChatExporter.Domain.Exporting
{
    public class RenderContext
    {
        public Guild Guild { get; }

        public Channel Channel { get; }

        public DateTimeOffset? After { get; }

        public DateTimeOffset? Before { get; }

        public string DateFormat { get; }

        public IReadOnlyCollection<User> MentionableUsers { get; }

        public IReadOnlyCollection<Channel> MentionableChannels { get; }

        public IReadOnlyCollection<Role> MentionableRoles { get; }

        public RenderContext(
            Guild guild,
            Channel channel,
            DateTimeOffset? after,
            DateTimeOffset? before,
            string dateFormat,
            IReadOnlyCollection<User> mentionableUsers,
            IReadOnlyCollection<Channel> mentionableChannels,
            IReadOnlyCollection<Role> mentionableRoles)

        {
            Guild = guild;
            Channel = channel;
            After = after;
            Before = before;
            DateFormat = dateFormat;
            MentionableUsers = mentionableUsers;
            MentionableChannels = mentionableChannels;
            MentionableRoles = mentionableRoles;
        }
    }
}
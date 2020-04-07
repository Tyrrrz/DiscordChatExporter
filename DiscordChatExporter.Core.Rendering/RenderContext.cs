using System;
using System.Collections.Generic;
using DiscordChatExporter.Core.Models;

namespace DiscordChatExporter.Core.Rendering
{
    public class RenderContext
    {
        public Guild Guild { get; }

        public Channel Channel { get; }

        public DateTimeOffset? After { get; }

        public DateTimeOffset? Before { get; }

        public string DateFormat { get; }

        public bool IsUseUtcEnabled { get; }

        public IReadOnlyCollection<User> MentionableUsers { get; }

        public IReadOnlyCollection<Channel> MentionableChannels { get; }

        public IReadOnlyCollection<Role> MentionableRoles { get; }

        public RenderContext(Guild guild, Channel channel, DateTimeOffset? after, DateTimeOffset? before, string dateFormat, bool isUseUtcEnabled,
            IReadOnlyCollection<User> mentionableUsers, IReadOnlyCollection<Channel> mentionableChannels, IReadOnlyCollection<Role> mentionableRoles)
        {
            Guild = guild;
            Channel = channel;
            After = after;
            Before = before;
            DateFormat = dateFormat;
            IsUseUtcEnabled = isUseUtcEnabled;
            MentionableUsers = mentionableUsers;
            MentionableChannels = mentionableChannels;
            MentionableRoles = mentionableRoles;
        }
    }
}
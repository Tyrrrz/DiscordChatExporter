using System.Collections.Generic;

namespace DiscordChatExporter.Core.Models
{
    public class ChannelChatLogContext
    {
        public Guild Guild { get; }

        public IReadOnlyList<Channel> GuildChannels { get; }

        public IReadOnlyList<Role> GuildRoles { get; }

        public Channel Channel { get; }

        public IReadOnlyList<User> Participants { get; }

        public ChannelChatLogContext(Guild guild,
            IReadOnlyList<Channel> guildChannels, IReadOnlyList<Role> guildRoles,
            Channel channel, IReadOnlyList<User> participants)
        {
            Guild = guild;
            GuildChannels = guildChannels;
            GuildRoles = guildRoles;
            Channel = channel;
            Participants = participants;
        }
    }
}
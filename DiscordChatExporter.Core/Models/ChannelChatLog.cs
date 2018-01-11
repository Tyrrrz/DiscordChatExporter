using System.Collections.Generic;

namespace DiscordChatExporter.Core.Models
{
    public class ChannelChatLog
    {
        public Guild Guild { get; }

        public Channel Channel { get; }

        public IReadOnlyList<MessageGroup> MessageGroups { get; }

        public int TotalMessageCount { get; }

        public ChannelChatLog(Guild guild, Channel channel, IReadOnlyList<MessageGroup> messageGroups,
            int totalMessageCount)
        {
            Guild = guild;
            Channel = channel;
            MessageGroups = messageGroups;
            TotalMessageCount = totalMessageCount;
        }
    }
}
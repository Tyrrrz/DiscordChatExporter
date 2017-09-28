using System.Collections.Generic;
using System.Linq;

namespace DiscordChatExporter.Models
{
    public class ChannelChatLog
    {
        public Guild Guild { get; }

        public Channel Channel { get; }

        public IReadOnlyList<MessageGroup> MessageGroups { get; }

        public ChannelChatLog(Guild guild, Channel channel, IEnumerable<MessageGroup> messageGroups)
        {
            Guild = guild;
            Channel = channel;
            MessageGroups = messageGroups.ToArray();
        }
    }
}
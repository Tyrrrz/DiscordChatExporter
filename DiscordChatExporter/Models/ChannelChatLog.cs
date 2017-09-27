using System.Collections.Generic;
using System.Linq;

namespace DiscordChatExporter.Models
{
    public class ChannelChatLog
    {
        public Guild Guild { get; }

        public Channel Channel { get; }

        public IReadOnlyList<Message> Messages { get; }

        public ChannelChatLog(Guild guild, Channel channel, IEnumerable<Message> messages)
        {
            Guild = guild;
            Channel = channel;
            Messages = messages.ToArray();
        }
    }
}
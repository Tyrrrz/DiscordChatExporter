using System.Collections.Generic;
using System.Linq;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Models
{
    public class ChatLog
    {
        public string ChannelId { get; }

        public IReadOnlyList<User> Participants { get; }

        public IReadOnlyList<Message> Messages { get; }

        public ChatLog(string channelId, IEnumerable<Message> messages)
        {
            ChannelId = channelId;
            Messages = messages.ToArray();
            Participants = Messages.Select(m => m.Author).Distinct(a => a.Name).ToArray();
        }
    }
}
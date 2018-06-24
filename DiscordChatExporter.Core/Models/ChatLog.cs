using System.Collections.Generic;
using System.Linq;

namespace DiscordChatExporter.Core.Models
{
    public class ChatLog
    {
        public Guild Guild { get; }

        public Channel Channel { get; }

        public IReadOnlyList<MessageGroup> MessageGroups { get; }

        public int TotalMessageCount => MessageGroups.Sum(g => g.Messages.Count);

        public Mentionables Mentionables { get; }

        public ChatLog(Guild guild, Channel channel, IReadOnlyList<MessageGroup> messageGroups,
            Mentionables mentionables)
        {
            Guild = guild;
            Channel = channel;
            MessageGroups = messageGroups;
            Mentionables = mentionables;
        }

        public override string ToString() => $"{Guild.Name} | {Channel.Name}";
    }
}
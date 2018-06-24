using System.Collections.Generic;
using System.Linq;

namespace DiscordChatExporter.Core.Models
{
    public class ChannelChatLog
    {
        public ChannelChatLogContext Context { get; }

        public IReadOnlyList<MessageGroup> MessageGroups { get; }

        public int TotalMessageCount => MessageGroups.Sum(g => g.Messages.Count);

        public ChannelChatLog(ChannelChatLogContext context, IReadOnlyList<MessageGroup> messageGroups)
        {
            Context = context;
            MessageGroups = messageGroups;
        }

        public override string ToString() => $"{Context.Guild.Name} | {Context.Channel.Name}";
    }
}
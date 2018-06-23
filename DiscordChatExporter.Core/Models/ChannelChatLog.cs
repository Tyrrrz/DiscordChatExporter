using System.Collections.Generic;

namespace DiscordChatExporter.Core.Models
{
    public class ChannelChatLog
    {
        public Guild Guild { get; }

        public Channel Channel { get; }

        public IReadOnlyList<MessageGroup> MessageGroups { get; }

        public int TotalMessageCount { get; }

        public Mentionables Mentionables { get; }

        public ChannelChatLog(Guild guild, Channel channel, IReadOnlyList<MessageGroup> messageGroups,
            int totalMessageCount, Mentionables mentionables)
        {
            Guild = guild;
            Channel = channel;
            MessageGroups = messageGroups;
            TotalMessageCount = totalMessageCount;
            Mentionables = mentionables;
        }

        public override string ToString() => $"{Guild.Name} | {Channel.Name} | {TotalMessageCount} messages";
    }
}
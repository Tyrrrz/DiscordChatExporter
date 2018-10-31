using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordChatExporter.Core.Models
{
    public class ChatLog
    {
        public Guild Guild { get; }

        public Channel Channel { get; }

        public DateTime? From { get; }

        public DateTime? To { get; }

        public IReadOnlyList<MessageGroup> MessageGroups { get; }

        public long TotalMessageCount { get; }

        public Mentionables Mentionables { get; }

        public ChatLog(Guild guild, Channel channel, DateTime? from, DateTime? to,
            IReadOnlyList<MessageGroup> messageGroups, long totalMessageCount, Mentionables mentionables)
        {
            Guild = guild;
            Channel = channel;
            From = from;
            To = to;
            MessageGroups = messageGroups;
            TotalMessageCount = totalMessageCount;
            Mentionables = mentionables;
        }

        public override string ToString() => $"{Guild.Name} | {Channel.Name}";
    }
}
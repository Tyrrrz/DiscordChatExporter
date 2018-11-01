using System;
using System.Collections.Generic;

namespace DiscordChatExporter.Core.Models
{
    public class ChatLog
    {
        public Guild Guild { get; }

        public Channel Channel { get; }

        public DateTime? From { get; }

        public DateTime? To { get; }

        public IReadOnlyList<Message> Messages { get; }

        public Mentionables Mentionables { get; }

        public ChatLog(Guild guild, Channel channel, DateTime? from, DateTime? to,
            IReadOnlyList<Message> messages, Mentionables mentionables)
        {
            Guild = guild;
            Channel = channel;
            From = from;
            To = to;
            Messages = messages;
            Mentionables = mentionables;
        }

        public override string ToString() => $"{Guild.Name} | {Channel.Name}";
    }
}
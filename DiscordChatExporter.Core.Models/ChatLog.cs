using System;
using System.Collections.Generic;

namespace DiscordChatExporter.Core.Models
{
    public class ChatLog
    {
        public Guild Guild { get; }

        public Channel Channel { get; }

        public DateTimeOffset? After { get; }

        public DateTimeOffset? Before { get; }

        public IReadOnlyList<Message> Messages { get; }

        public Mentionables Mentionables { get; }

        public ChatLog(Guild guild, Channel channel, DateTimeOffset? after, DateTimeOffset? before,
            IReadOnlyList<Message> messages, Mentionables mentionables)
        {
            Guild = guild;
            Channel = channel;
            After = after;
            Before = before;
            Messages = messages;
            Mentionables = mentionables;
        }

        public override string ToString() => $"{Guild.Name} | {Channel.Name}";
    }
}
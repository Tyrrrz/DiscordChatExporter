using System;

namespace DiscordChatExporter.Core.Models
{
    public class Reaction
    {
        public int Count { get; }

        public bool Me { get; }

        public string Id { get; }

        public string Name { get; }

        public string Emoji { get; }

        public Reaction(int count, bool me, string id, string name) 
        {
            Count = count;
            Me = me;
            Id = id;
            Name = name;
            Emoji = (id == "" ? name : $"<:{name}:{id}>");
        }
    }
}
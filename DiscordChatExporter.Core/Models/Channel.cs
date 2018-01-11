﻿namespace DiscordChatExporter.Core.Models
{
    public partial class Channel
    {
        public string Id { get; }

        public string GuildId { get; }

        public string Name { get; }

        public string Topic { get; }

        public ChannelType Type { get; }

        public Channel(string id, string guildId, string name, string topic, ChannelType type)
        {
            Id = id;
            GuildId = guildId;
            Name = name;
            Topic = topic;
            Type = type;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public partial class Channel
    {
        public static Channel CreateDeletedChannel(string id)
        {
            return new Channel(id, null, "deleted-channel", null, ChannelType.GuildTextChat);
        }
    }
}
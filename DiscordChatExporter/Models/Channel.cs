namespace DiscordChatExporter.Models
{
    public class Channel
    {
        public string Id { get; }

        public string Name { get; }

        public string Topic { get; }

        public ChannelType Type { get; }

        public Channel(string id, string name, string topic, ChannelType type)
        {
            Id = id;
            Name = name;
            Topic = topic;
            Type = type;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
namespace DiscordChatExporter.Models
{
    public class Channel
    {
        public string Id { get; }

        public string Name { get; }

        public ChannelType Type { get; }

        public Channel(string id, string name, ChannelType type)
        {
            Id = id;
            Name = name;
            Type = type;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
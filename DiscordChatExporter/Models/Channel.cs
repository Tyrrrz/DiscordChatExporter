namespace DiscordChatExporter.Models
{
    public class Channel
    {
        public string Id { get; }

        public string Name { get; }

        public Channel(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
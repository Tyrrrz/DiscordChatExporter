namespace DiscordChatExporter.Models
{
    public class Guild
    {
        public string Id { get; }

        public string Name { get; }

        public Guild(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
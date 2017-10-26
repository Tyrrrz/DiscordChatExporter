namespace DiscordChatExporter.Models
{
    public class Role
    {
        public string Id { get; }

        public string Name { get; }

        public Role(string id, string name)
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
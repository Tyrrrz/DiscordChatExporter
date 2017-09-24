namespace DiscordChatExporter.Models
{
    public class Guild
    {
        public string Id { get; }

        public string Name { get; }

        public string IconHash { get; }

        public string IconUrl => $"https://cdn.discordapp.com/icons/{Id}/{IconHash}.png";

        public Guild(string id, string name, string iconHash)
        {
            Id = id;
            Name = name;
            IconHash = iconHash;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
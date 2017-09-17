using Tyrrrz.Extensions;

namespace DiscordChatExporter.Models
{
    public class User
    {
        public string Id { get; }

        public string Name { get; }

        public string AvatarHash { get; }

        public string AvatarUrl => AvatarHash.IsNotBlank()
            ? $"https://cdn.discordapp.com/avatars/{Id}/{AvatarHash}.png?size=256"
            : "https://discordapp.com/assets/6debd47ed13483642cf09e832ed0bc1b.png";

        public User(string id, string name, string avatarHash)
        {
            Id = id;
            Name = name;
            AvatarHash = avatarHash;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
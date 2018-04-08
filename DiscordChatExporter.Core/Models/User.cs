using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Models
{
    public class User
    {
        public string Id { get; }

        public int Discriminator { get; }

        public string Name { get; }

        public string FullName => $"{Name}#{Discriminator:0000}";

        public string AvatarHash { get; }

        public string DefaultAvatarHash => $"{Discriminator % 5}";

        public string AvatarUrl => AvatarHash.IsNotBlank()
            ? $"https://cdn.discordapp.com/avatars/{Id}/{AvatarHash}.png"
            : $"https://cdn.discordapp.com/embed/avatars/{DefaultAvatarHash}.png";

        public User(string id, int discriminator, string name, string avatarHash)
        {
            Id = id;
            Discriminator = discriminator;
            Name = name;
            AvatarHash = avatarHash;
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
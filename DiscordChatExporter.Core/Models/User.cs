using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Models
{
    // https://discordapp.com/developers/docs/topics/permissions#role-object

    public partial class User
    {
        public string Id { get; }

        public int Discriminator { get; }

        public string Name { get; }

        public string FullName => $"{Name}#{Discriminator:0000}";

        public string AvatarHash { get; }

        public string DefaultAvatarHash => $"{Discriminator % 5}";

        public string AvatarUrl => AvatarHash.IsNotBlank()
            ? (AvatarHash.Substring(0, 2) == "a_"
            ? $"https://cdn.discordapp.com/avatars/{Id}/{AvatarHash}.gif"
            : $"https://cdn.discordapp.com/avatars/{Id}/{AvatarHash}.png")
            : $"https://cdn.discordapp.com/embed/avatars/{DefaultAvatarHash}.png";

        public User(string id, int discriminator, string name, string avatarHash)
        {
            Id = id;
            Discriminator = discriminator;
            Name = name;
            AvatarHash = avatarHash;
        }

        public override string ToString() => FullName;
    }

    public partial class User
    {
        public static User CreateUnknownUser(string id) =>
            new User(id, 0, "Unknown", null);
    }
}
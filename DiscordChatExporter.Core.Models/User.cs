using System;

namespace DiscordChatExporter.Core.Models
{
    // https://discordapp.com/developers/docs/topics/permissions#role-object

    public partial class User
    {
        public string Id { get; }

        public int Discriminator { get; }

        public string Name { get; }

        public string FullName => $"{Name}#{Discriminator:0000}";

        public string DefaultAvatarHash => $"{Discriminator % 5}";

        public string AvatarHash { get; }

        public bool IsAvatarAnimated =>
            AvatarHash != null && AvatarHash.StartsWith("a_", StringComparison.Ordinal);

        public string AvatarUrl
        {
            get
            {
                // Custom avatar
                if (AvatarHash != null)
                {
                    // Animated
                    if (IsAvatarAnimated)
                        return $"https://cdn.discordapp.com/avatars/{Id}/{AvatarHash}.gif";

                    // Non-animated
                    return $"https://cdn.discordapp.com/avatars/{Id}/{AvatarHash}.png";
                }

                // Default avatar
                return $"https://cdn.discordapp.com/embed/avatars/{DefaultAvatarHash}.png";
            }
        }

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
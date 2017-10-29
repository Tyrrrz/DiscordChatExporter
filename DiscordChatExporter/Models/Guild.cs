using System.Collections.Generic;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Models
{
    public partial class Guild
    {
        public string Id { get; }

        public string Name { get; }

        public string IconHash { get; }

        public string IconUrl => IconHash.IsNotBlank()
            ? $"https://cdn.discordapp.com/icons/{Id}/{IconHash}.png"
            : "https://cdn.discordapp.com/embed/avatars/0.png";

        public IReadOnlyList<Role> Roles { get; }

        public Guild(string id, string name, string iconHash, IReadOnlyList<Role> roles)
        {
            Id = id;
            Name = name;
            IconHash = iconHash;
            Roles = roles;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public partial class Guild
    {
        public static Guild DirectMessages { get; } = new Guild("@me", "Direct Messages", null, new Role[0]);
    }
}
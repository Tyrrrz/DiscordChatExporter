using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Models
{
    // https://discordapp.com/developers/docs/resources/guild#guild-object

    public partial class Guild
    {
        public string Id { get; }

        public string Name { get; }

        public string IconHash { get; }

        public string IconUrl => IconHash.IsNotBlank()
            ? $"https://cdn.discordapp.com/icons/{Id}/{IconHash}.png"
            : "https://cdn.discordapp.com/embed/avatars/0.png";

        public Guild(string id, string name, string iconHash)
        {
            Id = id;
            Name = name;
            IconHash = iconHash;
        }

        public override string ToString() => Name;
    }

    public partial class Guild
    {
        public static Guild DirectMessages { get; } = new Guild("@me", "Direct Messages", null);
    }
}
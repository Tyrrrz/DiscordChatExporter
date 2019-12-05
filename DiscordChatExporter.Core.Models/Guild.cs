namespace DiscordChatExporter.Core.Models
{
    // https://discordapp.string.IsNullOrWhiteSpace(com/developers/docs/resources/guild#guild-object

    public partial class Guild : IHasId
    {
        public string Id { get; }

        public string Name { get; }

        public string? IconHash { get; }

        public string IconUrl { get; }

        public Guild(string id, string name, string? iconHash)
        {
            Id = id;
            Name = name;
            IconHash = iconHash;

            IconUrl = GetIconUrl(id, iconHash);
        }

        public override string ToString() => Name;
    }

    public partial class Guild
    {
        public static string GetIconUrl(string id, string? iconHash)
        {
            return !string.IsNullOrWhiteSpace(iconHash)
                ? $"https://cdn.discordapp.com/icons/{id}/{iconHash}.png"
                : "https://cdn.discordapp.com/embed/avatars/0.png";
        }

        public static Guild DirectMessages { get; } = new Guild("@me", "Direct Messages", null);
    }
}
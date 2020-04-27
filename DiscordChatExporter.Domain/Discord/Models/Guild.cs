using System.Text.Json;
using DiscordChatExporter.Domain.Discord.Models.Common;

namespace DiscordChatExporter.Domain.Discord.Models
{
    // https://discordapp.com/developers/docs/resources/guild#guild-object
    public partial class Guild : IHasId
    {
        public string Id { get; }

        public string Name { get; }

        public string IconUrl { get; }

        public Guild(string id, string name, string? iconHash)
        {
            Id = id;
            Name = name;

            IconUrl = GetIconUrl(id, iconHash);
        }

        public override string ToString() => Name;
    }

    public partial class Guild
    {
        public static Guild DirectMessages { get; } =
            new Guild("@me", "Direct Messages", null);

        private static string GetIconUrl(string id, string? iconHash) =>
            !string.IsNullOrWhiteSpace(iconHash)
                ? $"https://cdn.discordapp.com/icons/{id}/{iconHash}.png"
                : "https://cdn.discordapp.com/embed/avatars/0.png";

        public static Guild Parse(JsonElement json)
        {
            var id = json.GetProperty("id").GetString();
            var name = json.GetProperty("name").GetString();
            var iconHash = json.GetProperty("icon").GetString();

            return new Guild(id, name, iconHash);
        }
    }
}
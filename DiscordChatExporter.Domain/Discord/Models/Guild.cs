using System.Text.Json;
using DiscordChatExporter.Domain.Discord.Models.Common;

namespace DiscordChatExporter.Domain.Discord.Models
{
    // https://discord.com/developers/docs/resources/guild#guild-object
    public partial class Guild : IHasId
    {
        public string Id { get; }

        public string Name { get; }

        public string IconUrl { get; }

        public Guild(string id, string name, string iconUrl)
        {
            Id = id;
            Name = name;
            IconUrl = iconUrl;
        }

        public override string ToString() => Name;
    }

    public partial class Guild
    {
        public static Guild DirectMessages { get; } = new("@me", "Direct Messages", GetDefaultIconUrl());

        private static string GetDefaultIconUrl() =>
            "https://cdn.discordapp.com/embed/avatars/0.png";

        private static string GetIconUrl(string id, string iconHash) =>
            $"https://cdn.discordapp.com/icons/{id}/{iconHash}.png";

        public static Guild Parse(JsonElement json)
        {
            var id = json.GetProperty("id").GetString();
            var name = json.GetProperty("name").GetString();
            var iconHash = json.GetProperty("icon").GetString();

            var iconUrl = !string.IsNullOrWhiteSpace(iconHash)
                ? GetIconUrl(id, iconHash)
                : GetDefaultIconUrl();

            return new Guild(id, name, iconUrl);
        }
    }
}
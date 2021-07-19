using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Core.Discord.Data
{
    // https://discord.com/developers/docs/resources/guild#guild-object
    public partial class Guild : IHasId
    {
        public Snowflake Id { get; }

        public string Name { get; }

        public string IconUrl { get; }

        public Guild(Snowflake id, string name, string iconUrl)
        {
            Id = id;
            Name = name;
            IconUrl = iconUrl;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString() => Name;
    }

    public partial class Guild
    {
        public static Guild DirectMessages { get; } = new(
            Snowflake.Zero,
            "Direct Messages",
            GetDefaultIconUrl()
        );

        private static string GetDefaultIconUrl() =>
            "https://cdn.discordapp.com/embed/avatars/0.png";

        private static string GetIconUrl(Snowflake id, string iconHash) =>
            $"https://cdn.discordapp.com/icons/{id}/{iconHash}.png";

        public static Guild Parse(JsonElement json)
        {
            var id = json.GetProperty("id").GetString().Pipe(Snowflake.Parse);
            var name = json.GetProperty("name").GetString();
            var iconHash = json.GetProperty("icon").GetString();

            var iconUrl = !string.IsNullOrWhiteSpace(iconHash)
                ? GetIconUrl(id, iconHash)
                : GetDefaultIconUrl();

            return new Guild(id, name, iconUrl);
        }
    }
}
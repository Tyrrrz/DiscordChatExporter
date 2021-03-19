using System;
using System.Text.Json;
using DiscordChatExporter.Core.Discord.Data.Common;
using DiscordChatExporter.Core.Utils.Extensions;
using JsonExtensions.Reading;

namespace DiscordChatExporter.Core.Discord.Data
{
    // https://discord.com/developers/docs/resources/user#user-object
    public partial class User : IHasId
    {
        public Snowflake Id { get; }

        public bool IsBot { get; }

        public int Discriminator { get; }

        public string DiscriminatorFormatted => $"{Discriminator:0000}";

        public string Name { get; }

        public string FullName => $"{Name}#{DiscriminatorFormatted}";

        public string AvatarUrl { get; }

        public User(
            Snowflake id,
            bool isBot,
            int discriminator,
            string name,
            string avatarUrl)
        {
            Id = id;
            IsBot = isBot;
            Discriminator = discriminator;
            Name = name;
            AvatarUrl = avatarUrl;
        }

        public override string ToString() => FullName;
    }

    public partial class User
    {
        private static string GetDefaultAvatarUrl(int discriminator) =>
            $"https://cdn.discordapp.com/embed/avatars/{discriminator % 5}.png";

        private static string GetAvatarUrl(Snowflake id, string avatarHash)
        {
            // Animated
            if (avatarHash.StartsWith("a_", StringComparison.Ordinal))
                return $"https://cdn.discordapp.com/avatars/{id}/{avatarHash}.gif";

            // Non-animated
            return $"https://cdn.discordapp.com/avatars/{id}/{avatarHash}.png";
        }

        public static User Parse(JsonElement json)
        {
            var id = json.GetProperty("id").GetString().Pipe(Snowflake.Parse);
            var discriminator = json.GetProperty("discriminator").GetString().Pipe(int.Parse);
            var name = json.GetProperty("username").GetString();
            var avatarHash = json.GetProperty("avatar").GetString();
            var isBot = json.GetPropertyOrNull("bot")?.GetBoolean() ?? false;

            var avatarUrl = !string.IsNullOrWhiteSpace(avatarHash)
                ? GetAvatarUrl(id, avatarHash)
                : GetDefaultAvatarUrl(discriminator);

            return new User(id, isBot, discriminator, name, avatarUrl);
        }
    }
}
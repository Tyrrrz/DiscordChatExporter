using System;
using System.Text.Json;
using DiscordChatExporter.Domain.Discord.Models.Common;
using DiscordChatExporter.Domain.Internal;

namespace DiscordChatExporter.Domain.Discord.Models
{
    // https://discordapp.com/developers/docs/resources/user#user-object
    public partial class User : IHasId
    {
        public string Id { get; }

        public bool IsBot { get; }

        public int Discriminator { get; }

        public string Name { get; }

        public string FullName => $"{Name}#{Discriminator:0000}";

        public string AvatarUrl { get; }

        public User(string id, bool isBot, int discriminator, string name, string? avatarHash)
        {
            Id = id;
            IsBot = isBot;
            Discriminator = discriminator;
            Name = name;

            AvatarUrl = GetAvatarUrl(id, discriminator, avatarHash);
        }

        public override string ToString() => FullName;
    }

    public partial class User
    {
        private static string GetAvatarUrl(string id, int discriminator, string? avatarHash)
        {
            // Custom avatar
            if (!string.IsNullOrWhiteSpace(avatarHash))
            {
                // Animated
                if (avatarHash.StartsWith("a_", StringComparison.Ordinal))
                    return $"https://cdn.discordapp.com/avatars/{id}/{avatarHash}.gif";

                // Non-animated
                return $"https://cdn.discordapp.com/avatars/{id}/{avatarHash}.png";
            }

            // Default avatar
            return $"https://cdn.discordapp.com/embed/avatars/{discriminator % 5}.png";
        }

        public static User Parse(JsonElement json)
        {
            var id = json.GetProperty("id").GetString();
            var discriminator = json.GetProperty("discriminator").GetString().Pipe(int.Parse);
            var name = json.GetProperty("username").GetString();
            var avatarHash = json.GetProperty("avatar").GetString();
            var isBot = json.GetPropertyOrNull("bot")?.GetBoolean() ?? false;

            return new User(id, isBot, discriminator, name, avatarHash);
        }
    }
}
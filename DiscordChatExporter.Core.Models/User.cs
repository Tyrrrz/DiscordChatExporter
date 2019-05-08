﻿using System;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Core.Models
{
    // https://discordapp.com/developers/docs/topics/permissions#role-object

    public partial class User
    {
        public string Id { get; }

        public int Discriminator { get; }

        public string Name { get; }

        public string FullName { get; }

        public string AvatarHash { get; }

        public string AvatarUrl { get; }

        public bool IsBot { get; }

        public User(string id, int discriminator, string name, string avatarHash, bool isBot)
        {
            Id = id;
            Discriminator = discriminator;
            Name = name;
            AvatarHash = avatarHash;
            IsBot = isBot;

            FullName = GetFullName(name, discriminator);
            AvatarUrl = GetAvatarUrl(id, discriminator, avatarHash);
        }

        public override string ToString() => FullName;
    }

    public partial class User
    {
        public static string GetFullName(string name, int discriminator) => $"{name}#{discriminator:0000}";

        public static string GetAvatarUrl(string id, int discriminator, string avatarHash)
        {
            // Custom avatar
            if (!avatarHash.IsNullOrWhiteSpace())
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

        public static User CreateUnknownUser(string id) => new User(id, 0, "Unknown", null, false);
    }
}
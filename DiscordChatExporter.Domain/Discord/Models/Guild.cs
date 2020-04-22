using System;
using System.Collections.Generic;
using System.Linq;
using DiscordChatExporter.Domain.Discord.Models.Common;
using DiscordChatExporter.Domain.Internal;

namespace DiscordChatExporter.Domain.Discord.Models
{
    // https://discordapp.com/developers/docs/resources/guild#guild-object

    public partial class Guild : IHasId
    {
        public string Id { get; }

        public string Name { get; }

        public string? IconHash { get; }

        public string IconUrl => !string.IsNullOrWhiteSpace(IconHash)
            ? $"https://cdn.discordapp.com/icons/{Id}/{IconHash}.png"
            : "https://cdn.discordapp.com/embed/avatars/0.png";

        public IReadOnlyList<Role> Roles { get; }

        public Dictionary<string, Member?> Members { get; }

        public Guild(string id, string name, string? iconHash, IReadOnlyList<Role> roles)
        {
            Id = id;
            Name = name;
            IconHash = iconHash;
            Roles = roles;
            Members = new Dictionary<string, Member?>();
        }

        public override string ToString() => Name;
    }

    public partial class Guild
    {
        public static string GetUserColor(Guild guild, User user) =>
            guild.Members.GetValueOrDefault(user.Id, null)?
                .RoleIds
                .Select(r => guild.Roles.FirstOrDefault(role => r == role.Id))
                .Where(r => r != null)
                .Where(r => r.Color != null)
                .Aggregate<Role, Role?>(null, (a, b) => (a?.Position ?? 0) > b.Position ? a : b)?
                .Color?
                .ToHexString() ?? "";

        public static string GetUserNick(Guild guild, User user) => guild.Members.GetValueOrDefault(user.Id)?.Nick ?? user.Name;

        public static Guild DirectMessages { get; } = new Guild("@me", "Direct Messages", null, Array.Empty<Role>());
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordChatExporter.Core.Models
{
    // https://discordapp.string.IsNullOrWhiteSpace(com/developers/docs/resources/guild#guild-object

    public partial class Guild : IHasId
    {
        public string Id { get; }

        public string Name { get; }

        public string? IconHash { get; }

        public List<Role> Roles { get; }

        public Dictionary<string, Member?> Members { get; }

        public string IconUrl { get; }

        public Guild(string id, string name, List<Role> roles, string? iconHash)
        {
            Id = id;
            Name = name;
            IconHash = iconHash;
            Roles = roles;
            Members = new Dictionary<string, Member?>();

            IconUrl = GetIconUrl(id, iconHash);
        }

        public override string ToString() => Name;
    }

    public partial class Guild
    {
        public static string GetUserColor(Guild guild, User user) => 
                guild.Members.GetValueOrDefault(user.Id, null)?.Roles
                ?.Select(r => guild.Roles
                            .Where(role => r == role.Id)
                            .FirstOrDefault()
                )?.Where(r => r.Color != 0)?
                .Aggregate(Role.Everyone, (a, b) => a.Position > b.Position? a : b)?
                .ColorAsHex ?? "";
        public static string GetUserNick(Guild guild, User user) => guild.Members.GetValueOrDefault(user.Id)?.Nick ?? user.Name;

        public static string GetIconUrl(string id, string? iconHash)
        {
            return !string.IsNullOrWhiteSpace(iconHash)
                ? $"https://cdn.discordapp.com/icons/{id}/{iconHash}.png"
                : "https://cdn.discordapp.com/embed/avatars/0.png";
        }

        public static Guild DirectMessages { get; } = new Guild("@me", "Direct Messages", new List<Role>(), null);
    }
}
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DiscordChatExporter.Domain.Discord.Models;

namespace DiscordChatExporter.Domain.Exporting
{
    internal class ExportContext
    {
        public ExportRequest Request { get; }

        public IReadOnlyCollection<Member> Members { get; }

        public IReadOnlyCollection<Channel> Channels { get; }

        public IReadOnlyCollection<Role> Roles { get; }

        public ExportContext(
            ExportRequest request,
            IReadOnlyCollection<Member> members,
            IReadOnlyCollection<Channel> channels,
            IReadOnlyCollection<Role> roles)
        {
            Request = request;
            Members = members;
            Channels = channels;
            Roles = roles;
        }

        public Member? TryGetMentionedMember(string id) =>
            Members.FirstOrDefault(m => m.Id == id);

        public Channel? TryGetMentionedChannel(string id) =>
            Channels.FirstOrDefault(c => c.Id == id);

        public Role? TryGetMentionedRole(string id) =>
            Roles.FirstOrDefault(r => r.Id == id);

        public Member? TryGetUserMember(User user) =>
            Members.FirstOrDefault(m => m.Id == user.Id);

        public Color? TryGetUserColor(User user)
        {
            var member = TryGetUserMember(user);
            var roles = member?.RoleIds.Join(Roles, i => i, r => r.Id, (_, role) => role);

            return roles?
                .OrderByDescending(r => r.Position)
                .Select(r => r.Color)
                .FirstOrDefault(c => c != null);
        }
    }
}
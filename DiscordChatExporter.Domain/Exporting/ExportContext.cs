using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DiscordChatExporter.Domain.Discord.Models;

namespace DiscordChatExporter.Domain.Exporting
{
    internal class ExportContext
    {
        private readonly MediaDownloader _mediaDownloader;

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

            _mediaDownloader = new MediaDownloader(request.OutputMediaDirPath);
        }

        public Member? TryGetMember(string id) =>
            Members.FirstOrDefault(m => m.Id == id);

        public Channel? TryGetChannel(string id) =>
            Channels.FirstOrDefault(c => c.Id == id);

        public Role? TryGetRole(string id) =>
            Roles.FirstOrDefault(r => r.Id == id);

        public Color? TryGetUserColor(User user)
        {
            var member = TryGetMember(user.Id);
            var roles = member?.RoleIds.Join(Roles, i => i, r => r.Id, (_, role) => role);

            return roles?
                .Where(r => r.Color != null)
                .OrderByDescending(r => r.Position)
                .Select(r => r.Color)
                .FirstOrDefault();
        }

        // HACK: ConfigureAwait() is crucial here to enable sync-over-async in HtmlMessageWriter
        public async ValueTask<string> ResolveMediaUrlAsync(string url)
        {
            if (!Request.ShouldDownloadMedia)
                return url;

            try
            {
                var filePath = await _mediaDownloader.DownloadAsync(url).ConfigureAwait(false);

                // Return relative path so that the output files can be copied around without breaking
                return Path.GetRelativePath(Request.OutputBaseDirPath, filePath);
            }
            catch (HttpRequestException)
            {
                // We don't want this to crash the exporting process in case of failure
                return url;
            }
        }
    }
}
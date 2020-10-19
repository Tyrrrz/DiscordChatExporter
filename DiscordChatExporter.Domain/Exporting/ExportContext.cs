using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DiscordChatExporter.Domain.Discord.Models;
using DiscordChatExporter.Domain.Internal.Extensions;
using Tyrrrz.Extensions;

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

        public string FormatDate(DateTimeOffset date) => Request.DateFormat switch
        {
            "unix" => date.ToUnixTimeSeconds().ToString(),
            "unixms" => date.ToUnixTimeMilliseconds().ToString(),
            var df => date.ToLocalString(df),
        };

        public Member? TryGetMember(string id) =>
            Members.FirstOrDefault(m => m.Id == id);

        public Channel? TryGetChannel(string id) =>
            Channels.FirstOrDefault(c => c.Id == id);

        public Role? TryGetRole(string id) =>
            Roles.FirstOrDefault(r => r.Id == id);

        public Color? TryGetUserColor(string id)
        {
            var member = TryGetMember(id);
            var roles = member?.RoleIds.Join(Roles, i => i, r => r.Id, (_, role) => role);

            return roles?
                .Where(r => r.Color != null)
                .OrderByDescending(r => r.Position)
                .Select(r => r.Color)
                .FirstOrDefault();
        }

        public async ValueTask<string> ResolveMediaUrlAsync(string url)
        {
            return await DownloadMediaAsync(url, false);
        }

        public async ValueTask<string> ResolveMediaUrlForHtmlAsync(string url)
        {
            return await DownloadMediaAsync(url, true);
        }

        private async ValueTask<string> DownloadMediaAsync(string url, bool forHtmlOutput)
        {
            if (!Request.ShouldDownloadMedia)
                return url;

            try
            {
                var filePath = await _mediaDownloader.DownloadAsync(url);

                // We want relative path so that the output files can be copied around without breaking
                var relativeFilePath = Path.GetRelativePath(Request.OutputBaseDirPath, filePath);

                // For HTML, we need to format the URL properly
                if (forHtmlOutput)
                {
                    // Need to escape each path segment while keeping the directory separators intact
                    return relativeFilePath
                        .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                        .Select(Uri.EscapeDataString)
                        .JoinToString(Path.AltDirectorySeparatorChar.ToString());
                }

                return relativeFilePath;
            }
            // Try to catch only exceptions related to failed HTTP requests
            // https://github.com/Tyrrrz/DiscordChatExporter/issues/332
            // https://github.com/Tyrrrz/DiscordChatExporter/issues/372
            catch (Exception ex) when (ex is HttpRequestException || ex is OperationCanceledException)
            {
                // We don't want this to crash the exporting process in case of failure
                return url;
            }
        }
    }
}
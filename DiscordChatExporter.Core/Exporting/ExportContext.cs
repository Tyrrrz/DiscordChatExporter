﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Core.Exporting;

internal class ExportContext
{
    private readonly ExportAssetDownloader _assetDownloader;

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

        _assetDownloader = new ExportAssetDownloader(request.OutputAssetsDirPath, request.ShouldReuseAssets);
    }

    public string FormatDate(DateTimeOffset date) => Request.DateFormat switch
    {
        "unix" => date.ToUnixTimeSeconds().ToString(),
        "unixms" => date.ToUnixTimeMilliseconds().ToString(),
        var format => date.ToLocalString(format)
    };

    public Member? TryGetMember(Snowflake id) => Members.FirstOrDefault(m => m.Id == id);

    public Channel? TryGetChannel(Snowflake id) => Channels.FirstOrDefault(c => c.Id == id);

    public Role? TryGetRole(Snowflake id) => Roles.FirstOrDefault(r => r.Id == id);

    public Color? TryGetUserColor(Snowflake id)
    {
        var member = TryGetMember(id);
        var roles = member?.RoleIds.Join(Roles, i => i, r => r.Id, (_, role) => role);

        return roles?
            .Where(r => r.Color is not null)
            .OrderByDescending(r => r.Position)
            .Select(r => r.Color)
            .FirstOrDefault();
    }

    public async ValueTask<string> ResolveAssetUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        if (!Request.ShouldDownloadAssets)
            return url;

        try
        {
            var absoluteFilePath = await _assetDownloader.DownloadAsync(url, cancellationToken);

            // We want relative path so that the output files can be copied around without breaking.
            // Base directory path may be null if the file is stored at the root or relative to working directory.
            var relativeFilePath = !string.IsNullOrWhiteSpace(Request.OutputBaseDirPath)
                ? Path.GetRelativePath(Request.OutputBaseDirPath, absoluteFilePath)
                : absoluteFilePath;

            // If the assets path is outside of the export directory, fall back to absolute path
            var filePath = relativeFilePath.StartsWith("..")
                ? absoluteFilePath
                : relativeFilePath;


            // HACK: for HTML, we need to format the URL properly
            if (Request.Format is ExportFormat.HtmlDark or ExportFormat.HtmlLight)
            {
                // Need to escape each path segment while keeping the directory separators intact
                return string.Join(
                    Path.AltDirectorySeparatorChar,
                    filePath
                        .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                        .Select(Uri.EscapeDataString)
                        .Select(x => x.Replace("%3A", ":"))
                );
            }

            return filePath;
        }
        // Try to catch only exceptions related to failed HTTP requests
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/332
        // https://github.com/Tyrrrz/DiscordChatExporter/issues/372
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException)
        {
            // TODO: add logging so we can be more liberal with catching exceptions
            // We don't want this to crash the exporting process in case of failure
            return url;
        }
    }
}
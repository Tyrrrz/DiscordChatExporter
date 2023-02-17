using System;
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
    private readonly Dictionary<Snowflake, Member?> _members = new();
    private readonly Dictionary<Snowflake, Channel> _channels = new();
    private readonly Dictionary<Snowflake, Role> _roles = new();
    private readonly ExportAssetDownloader _assetDownloader;

    public DiscordClient Discord { get; }
    public ExportRequest Request { get; }

    public ExportContext(DiscordClient discord,
        ExportRequest request)
    {
        Discord = discord;
        Request = request;

        _assetDownloader = new ExportAssetDownloader(
            request.OutputAssetsDirPath,
            request.ShouldReuseAssets
        );
    }

    public async ValueTask PopulateChannelsAndRolesAsync(CancellationToken cancellationToken = default)
    {
        await foreach (var channel in Discord.GetGuildChannelsAsync(Request.Guild.Id, cancellationToken))
            _channels[channel.Id] = channel;

        await foreach (var role in Discord.GetGuildRolesAsync(Request.Guild.Id, cancellationToken))
            _roles[role.Id] = role;
    }

    // Because members are not pulled in bulk, we need to populate them on demand
    public async ValueTask PopulateMemberAsync(Snowflake id, CancellationToken cancellationToken = default)
    {
        if (_members.ContainsKey(id))
            return;

        var member = await Discord.TryGetGuildMemberAsync(Request.Guild.Id, id, cancellationToken);

        // Store the result even if it's null, to avoid re-fetching non-existing members
        _members[id] = member;
    }

    public string FormatDate(DateTimeOffset instant) => Request.DateFormat switch
    {
        "unix" => instant.ToUnixTimeSeconds().ToString(),
        "unixms" => instant.ToUnixTimeMilliseconds().ToString(),
        var format => instant.ToLocalString(format)
    };

    public Member? TryGetMember(Snowflake id) => _members.GetValueOrDefault(id);

    public Channel? TryGetChannel(Snowflake id) => _channels.GetValueOrDefault(id);

    public Role? TryGetRole(Snowflake id) => _roles.GetValueOrDefault(id);

    public Color? TryGetUserColor(Snowflake id)
    {
        var member = TryGetMember(id);

        var memberRoles = member?
            .RoleIds
            .Select(TryGetRole)
            .WhereNotNull()
            .ToArray();

        return memberRoles?
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